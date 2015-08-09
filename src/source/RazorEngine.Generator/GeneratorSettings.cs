using System.Collections.Generic;

namespace RazorEngine.Generator
{
    public class GeneratorSettings
    {
        public string BaseClass { get; set; }
        public HashSet<string> Namespaces { get; set; }

        public GeneratorSettings()
        {
            Namespaces = new HashSet<string>();
        }
    }
}