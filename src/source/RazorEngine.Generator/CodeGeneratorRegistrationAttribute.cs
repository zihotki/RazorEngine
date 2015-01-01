/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace RazorEngine.Generator
{
    // Note, that class shipped as source code with the earlier Visual Studio SDKs.
    // However, this same functionality is now included in the later VS Shell assemblies.
    // 
    //    Microsoft.VisualStudio.Shell.11.0.dll
    //    Microsoft.VisualStudio.Shell.12.0.dll

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class CodeGeneratorRegistrationAttribute : RegistrationAttribute
    {
        /// <summary>
        /// Get the generator Type
        /// </summary>
        public Type GeneratorType { get; private set; }

        /// <summary>
        /// Get the Guid representing the project type
        /// </summary>
        public string ContextGuid { get; private set; }

        /// <summary>
        /// Get the Guid representing the generator type
        /// </summary>
        public Guid GeneratorGuid { get; private set; }

        /// <summary>
        /// Get or Set the GeneratesDesignTimeSource value
        /// </summary>
        public bool GeneratesDesignTimeSource { get; set; }

        /// <summary>
        /// Get or Set the GeneratesSharedDesignTimeSource value
        /// </summary>
        public bool GeneratesSharedDesignTimeSource { get; set; }


        /// <summary>
        /// Gets the Generator name 
        /// </summary>
        public string GeneratorName { get; private set; }

        /// <summary>
        /// Gets the Generator reg key name under 
        /// </summary>
        public string GeneratorRegKeyName { get; set; }

        /// <summary>
        /// Property that gets the generator base key name
        /// </summary>
        private string GeneratorRegKey
        {
            get { return string.Format(CultureInfo.InvariantCulture, @"Generators\{0}\{1}", ContextGuid, GeneratorRegKeyName); }
        }

        public CodeGeneratorRegistrationAttribute(Type generatorType, string generatorName, string contextGuid)
        {
            GeneratesSharedDesignTimeSource = false;
            GeneratesDesignTimeSource = false;
            if (generatorType == null)
                throw new ArgumentNullException("generatorType");
            if (generatorName == null)
                throw new ArgumentNullException("generatorName");
            if (contextGuid == null)
                throw new ArgumentNullException("contextGuid");

            ContextGuid = contextGuid;
            GeneratorType = generatorType;
            GeneratorName = generatorName;
            GeneratorRegKeyName = generatorType.Name;
            GeneratorGuid = generatorType.GUID;
        }

        /// <summary>
        ///     Called to register this attribute with the given context.  The context
        ///     contains the location where the registration inforomation should be placed.
        ///     It also contains other information such as the type being registered and path information.
        /// </summary>
        public override void Register(RegistrationContext context)
        {
            using (var childKey = context.CreateKey(GeneratorRegKey))
            {
                childKey.SetValue(string.Empty, GeneratorName);
                childKey.SetValue("CLSID", GeneratorGuid.ToString("B"));

                if (GeneratesDesignTimeSource)
                    childKey.SetValue("GeneratesDesignTimeSource", 1);

                if (GeneratesSharedDesignTimeSource)
                    childKey.SetValue("GeneratesSharedDesignTimeSource", 1);

            }
        }

        /// <summary>
        /// Unregister this file extension.
        /// </summary>
        /// <param name="context"></param>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(GeneratorRegKey);
        }
    }
}
