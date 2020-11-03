using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using System;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    public class CodeDependencyReader
    {
        public CodeDependency ExtractData(string text)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return new CodeDependency
            {
                Expression = ExtractNamespace(root, text)
            };
        }
        private static string ExtractNamespace(CompilationUnitSyntax root, string text)
        {
            var namespaceDeclaration = root.DescendantNodes(null, false)
                .OfType<NamespaceDeclarationSyntax>().First();

            var textSlice = text.AsSpan(start: namespaceDeclaration.OpenBraceToken.SpanStart + 1, 
                namespaceDeclaration.CloseBraceToken.SpanStart - namespaceDeclaration.OpenBraceToken.SpanStart - 1);

            return textSlice.ToString().Trim();
        }
    }
}
