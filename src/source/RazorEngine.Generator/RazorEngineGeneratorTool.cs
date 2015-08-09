using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Razor.Parser;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace RazorEngine.Generator
{
    [ComVisible(true)]
    public abstract class RazorEngineGeneratorTool : CustomToolBase
    {
        protected abstract override string DefaultExtension();

        protected override byte[] Generate(string inputFilePath, string inputFileContents, string defaultNamespace,
            IVsGeneratorProgress progressCallback)
        {
            return System.Text.Encoding.UTF8.GetBytes(GenerateCode(inputFilePath, inputFileContents, defaultNamespace, progressCallback));
        }

        private static string GenerateCode(string inputFilePath, string inputFileContents, string defaultNamespace,
            IVsGeneratorProgress progressCallback)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFilePath);
                var className = ParserHelpers.SanitizeClassName(fileName);

                var templateServiceConfig = new TemplateServiceConfiguration
                {
                    Language = Language.CSharp,
                    Debug = false,
                };

                var config = FindAndParseConfigsForFile(inputFilePath);
                if (config.Namespaces.Any())
                {
                    foreach (var ns in config.Namespaces)
                    {
                        templateServiceConfig.Namespaces.Add(ns);
                    }
                }

                using (var service = new TemplateService(templateServiceConfig))
                {
                    var code = service.GenerateCode(inputFileContents, className, defaultNamespace);

                    if (string.IsNullOrWhiteSpace(config.BaseClass) == false)
                    {
                        code.Replace("RazorEngine.Templating.TemplateBase", config.BaseClass);
                    }

                    return code.ToString();
                }
            }
            catch (TemplateParsingException e)
            {
                progressCallback.GeneratorError(0, 0, e.Message, (uint)e.Line, (uint)e.Column);
            }
            catch (Exception e)
            {
                progressCallback.GeneratorError(0, 0, e.ToString(), 1, 1);
            }

            return string.Empty;
        }

        private static GeneratorSettings FindAndParseConfigsForFile(string inputFilePath)
        {
            var settings = new GeneratorSettings();
            var webConfigs = ScanDirUp(inputFilePath);

            foreach (var webConfig in webConfigs)
            {
                var configContent = File.ReadAllText(webConfig);
                if (string.IsNullOrWhiteSpace(configContent))
                {
                    continue;
                }

                var config = XDocument.Parse(configContent);

                string razorSectionName;
                string pagesSectionName;
                
                ExtractSectionNames(config, out razorSectionName, out pagesSectionName);

                if (string.IsNullOrWhiteSpace(razorSectionName) || string.IsNullOrWhiteSpace(pagesSectionName))
                {
                    continue;
                }

                var pagesSection = config.Descendants(razorSectionName)
                    .Descendants(pagesSectionName)
                    .FirstOrDefault();

                if (pagesSection == null)
                {
                    continue;
                }

                var baseTypeAttr = pagesSection.Attribute("pageBaseType");
                if (baseTypeAttr != null && string.IsNullOrWhiteSpace(baseTypeAttr.Value) == false)
                {
                    settings.BaseClass = baseTypeAttr.Value;
                }

                var namespaces = pagesSection.Descendants("add");

                foreach (var ns in namespaces)
                {
                    var nsAttr = ns.Attribute("namespace");
                    if (nsAttr != null && string.IsNullOrWhiteSpace(nsAttr.Value) == false)
                    {
                        settings.Namespaces.Add(nsAttr.Value);
                    }
                }
            }

            return settings;
        }

        private static void ExtractSectionNames(XDocument config, out string razorSectionName, out string pagesSectionName)
        {
            razorSectionName = null;
            pagesSectionName = null;

            var sections = config.Descendants("sectionGroup").Where(x => x.HasAttributes);
            foreach (var section in sections)
            {
                var typeAttr = section.Attribute("type");
                if (typeAttr == null || typeAttr.Value.Contains("RazorWebSectionGroup") == false)
                {
                    continue;
                }
                var nameAttr = section.Attribute("name");
                if (nameAttr == null)
                {
                    continue;
                }

                razorSectionName = nameAttr.Value;
                var subSections = section.Descendants("section").Where(x => x.HasAttributes);

                foreach (var subSection in subSections)
                {
                    var subTypeAttr = subSection.Attribute("type");
                    if (subTypeAttr == null || subTypeAttr.Value.Contains("RazorPagesSection") == false)
                    {
                        continue;
                    }
                    var subNameAttr = subSection.Attribute("name");
                    if (subNameAttr == null)
                    {
                        continue;
                    }
                    pagesSectionName = subNameAttr.Value;
                    break;
                }
            }
        }
        
        private static string[] ScanDirUp(string inputFilePath)
        {
            DirectoryInfo dir = null;
            try
            {
                dir = new DirectoryInfo(Path.GetDirectoryName(inputFilePath));

                var configs = new List<string>();
                while (dir != null)
                {
                    var files = dir.GetFiles("web.config", SearchOption.TopDirectoryOnly);
                    if (files.Any())
                    {
                        configs.InsertRange(0, files.Select(x => x.FullName));
                    }

                    if (dir.GetFileSystemInfos("*.csproj").Any() || dir.GetFileSystemInfos("*.sln").Any())
                    {
                        break;
                    }

                    dir = dir.Parent;
                }

                return configs.ToArray();
            }
            catch (IOException e)
            {
                throw new RazorEnginegeneratorException(
                    string.Format("An exception occured when scanning directories, current dir is {0}",
                        dir == null ? "null" : dir.FullName),
                    e);
            }
        }
    }
}