using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using Omnia.CLI.Extensions;
namespace Omnia.CLI.Commands.Model.Behaviours
{
    public class ApplicationBehaviourReader
    {
        private const string BehaviourNamespacePrefix = "Omnia.Behaviours.";
        private static string[] DefaultUsings = new string[]
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Net",
            "Newtonsoft.Json",
            "System.Threading.Tasks",
            "Omnia.Libraries.Infrastructure.Behaviours",
            "Omnia.Libraries.Infrastructure.Connector",
            "Omnia.Libraries.Infrastructure.Connector.Client",
            "Omnia.Libraries.Infrastructure.Behaviours.Query",
            "Omnia.Libraries.Infrastructure.Behaviours.Action",
        };

        public ApplicationBehaviour ExtractData(string text)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return new ApplicationBehaviour(
                ExtractNamespace(root),
                ExtractExpression(root),
                ExtractUsings(root));
        }

        private static string ExtractNamespace(CompilationUnitSyntax root)
        {
            var namespaceDeclaration = root.DescendantNodes(null, false)
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclaration.Single().Name.ToString();
        }

        private static IList<string> ExtractUsings(CompilationUnitSyntax root)
        {
            return root.DescendantNodes(null, false)
                .OfType<UsingDirectiveSyntax>()
                .Select(GetDirectiveName)
                .Where(IsNotDefaultUsing)
                .ToList();

            static string GetDirectiveName(UsingDirectiveSyntax usingDirective)
                => usingDirective.Name.ToFullString();

            static bool IsNotDefaultUsing(string usingDirective)
                => !DefaultUsings.Contains(usingDirective) && !usingDirective.StartsWith(BehaviourNamespacePrefix);
        }

        private static string ExtractExpression(CompilationUnitSyntax root)
        {
            var blockText = root.DescendantNodes(null, false).OfType<BlockSyntax>().SingleOrDefault()?.ToFullString();
            return RemoveWithoutLeadingAndTrailingBraces(blockText).Trim();

            static string RemoveWithoutLeadingAndTrailingBraces(string blockText)
                => blockText
                    .Substring(0, blockText.LastIndexOf('}'))
                      .Substring(blockText.IndexOf('{') + 1);
                    
        }
    }
}