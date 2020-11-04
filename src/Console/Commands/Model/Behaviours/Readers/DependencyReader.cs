using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Omnia.CLI.Commands.Model.Behaviours.Readers
{
    public class DependencyReader
    {
        public CodeDependency ExtractCodeDependencies(string text)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return new CodeDependency
            {
                Namespace = ExtractNamespace(root),
                Expression = ExtractNamespaceBody(root, text)
            };
        }

        public IList<FileDependency> ExtractFileDependencies(string projectXml)
        {
            var doc = XDocument.Parse(projectXml);
            var references = doc.Descendants("ItemGroup")
                .SelectMany(group => group.Descendants("Reference"));

            var result = new List<FileDependency>();
            foreach (var reference in references)
            {
                var assemblyName = reference.Attribute("Include").Value;
                var path = reference.Descendants("HintPath").FirstOrDefault()?.Value;

                result.Add(new FileDependency
                {
                    AssemblyName = assemblyName,
                    Path = path
                });
            }

            return result;

        }
        private static string ExtractNamespace(CompilationUnitSyntax root)
        {
            var namespaceDeclaration = root.DescendantNodes(null, false)
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclaration.First().Name.ToString();
        }
        private static string ExtractNamespaceBody(CompilationUnitSyntax root, string text)
        {
            var namespaceDeclaration = root.DescendantNodes(null, false)
                .OfType<NamespaceDeclarationSyntax>().First();

            var textSlice = text.AsSpan(start: namespaceDeclaration.OpenBraceToken.SpanStart + 1,
                namespaceDeclaration.CloseBraceToken.SpanStart - namespaceDeclaration.OpenBraceToken.SpanStart - 1);

            return textSlice.ToString().Trim();

        }
    }
}
