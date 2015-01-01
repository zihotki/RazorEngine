using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Razor.Parser;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace RazorEngine.Generator
{
    public static class vsContextGuids
    {
        public const string vsContextGuidVCSProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
        public const string vsContextGuidVCSEditor = "{694DD9B6-B865-4C5B-AD85-86356E9C88DC}";
        public const string vsContextGuidVBProject = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";
        public const string vsContextGuidVBEditor = "{E34ACDC0-BAAE-11D0-88BF-00A0C9110049}";
    }

    // Note: the class name is used as the name of the Custom Tool from the end-user's perspective.
    [ComVisible(true)]
    [Guid("9044ED75-89BF-4C04-896D-8EE2D4401783")]
    [CodeGeneratorRegistration(typeof(RazorEngineGenerator), "RazorEngine Generator to C#", vsContextGuids.vsContextGuidVCSProject,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(RazorEngineGenerator))]
    public class RazorEngineGenerator : RazorEngineGeneratorTool
    {
        protected override string DefaultExtension()
        {
            return ".generated.cs";
        }
    }

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

                using (var service = new TemplateService(new TemplateServiceConfiguration
                {
                    Language = Language.CSharp,
                    Debug = false
                }))
                {
                    var code = service.GenerateCode(inputFileContents, className, defaultNamespace);
                    return code;
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
    }
}