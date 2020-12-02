using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Apply.Data.Server;
using Omnia.CLI.Commands.Model.Apply.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply.Readers.Server
{
    public class ApplicationBehaviourReader
    {
        private const string BehaviourNamespacePrefix = "Omnia.Behaviours.";
        private static readonly string[] DefaultUsings = {
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

            var method = root.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .SingleOrDefault();

            var (name, description) = method.ExtractDataFromComment();

            return new ApplicationBehaviour(
                name,
                description,
                ExtractNamespace(root),
                method.ExtractExpression(),
                ExtractUsings(root));
        }

        private static string ExtractNamespace(CompilationUnitSyntax root)
        {
            var namespaceDeclaration = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclaration.Single().Name.ToString();
        }

        private static IList<string> ExtractUsings(CompilationUnitSyntax root)
        {
            return root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(GetDirectiveName)
                .Where(IsNotDefaultUsing)
                .ToList();

            static string GetDirectiveName(UsingDirectiveSyntax usingDirective)
                => usingDirective.Name.ToFullString();

            static bool IsNotDefaultUsing(string usingDirective)
                => !DefaultUsings.Contains(usingDirective) && !usingDirective.StartsWith(BehaviourNamespacePrefix);
        }
    }
}