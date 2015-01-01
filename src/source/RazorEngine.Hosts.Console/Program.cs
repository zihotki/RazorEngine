using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;
using RazorEngine.Compilation.Inspectors;

namespace RazorEngine.Hosts.Console
{
    using System;
    using System.Linq;

    using Compilation;
    using Templating;

    class Program
    {
        static void Main(string[] args)
        {
            var template = @"
@model Person

@using Suvoda.Core

@{
    Layout = ""~/EmailTemplates/Shared/_EmptyContentLayout.cshtml"";
}

<h1>Hello world, @Model.Name!</h1>

@section Footer 
{
    <section id='footer'>kappa!</section>
}
";

            using (var service = new TemplateService())
            {
                var code = service.GenerateCode(template, "Hello", "BlaBla.Bla");
                Console.Write(code);
            }

            
            Console.ReadKey();
        }
    }
}
