using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace RazorEngine.Generator
{
    // Note: the class name is used as the name of the Custom Tool from the end-user's perspective.
    [ComVisible(true)]
    [Guid("9044ED75-89BF-4C04-896D-8EE2D4401783")]
    [CodeGeneratorRegistration(typeof(RazorEngineGenerator), "RazorEngine Generator to C#", VsContextGuids.vsContextGuidVCSProject,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(RazorEngineGenerator))]
    public class RazorEngineGenerator : RazorEngineGeneratorTool
    {
        protected override string DefaultExtension()
        {
            return ".generated.cs";
        }
    }
}