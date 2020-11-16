using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Apply.Data;
using Omnia.CLI.Commands.Model.Apply.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply.Readers
{
    public class DaoReader
    {
        private const string BehaviourNamespacePrefix = "Omnia.Behaviours.";
        private static readonly string[] DefaultUsings =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Net",
            "System.Threading.Tasks",
            "Newtonsoft.Json",
            "Omnia.Libraries.Infrastructure.Connector",
            "Omnia.Libraries.Infrastructure.Connector.Client",
            "Omnia.Libraries.Infrastructure.Behaviours",
            "Omnia.Libraries.Infrastructure.Behaviours.Query",
            "Omnia.Libraries.Infrastructure.Behaviours.Action",
        };

        public Entity ExtractData(string text)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return new Entity(
                ExtractNamespace(root),
                null,
                ExtractMethods(root),
                ExtractUsings(root));
        }

        private static string ExtractNamespace(CompilationUnitSyntax root)
        {
            var namespaceDeclaration = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclaration.Single().Name.ToString();
        }

        private static IList<DataBehaviour> ExtractMethods(CompilationUnitSyntax root)
        {
            return root.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .Select(MapMethod)
                            .Where(HasExpression)
                            .ToList();

            static bool HasExpression(DataBehaviour m)
                => !string.IsNullOrEmpty(m.Expression);
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

        private static DataBehaviour MapMethod(MethodDeclarationSyntax method)
        {
            var (name, description) = method.ExtractDataFromComment();

            return new DataBehaviour
            {
                Expression = method.ExtractExpression(),
                Name = name ?? GetMethodName(method),
                Type = MapType(method),
                Description = description
            };
        }

        private static DataBehaviourType MapType(MethodDeclarationSyntax method)
        {

            return GetMethodName(method) switch
            {
                var create when create.Equals("Create") => DataBehaviourType.Create,
                var update when update.Equals("Update") => DataBehaviourType.Update,
                var delete when delete.Equals("Delete") => DataBehaviourType.Delete,
                var read when read.Equals("Read") => DataBehaviourType.Read,
                var readList when readList.Equals("ReadList") => DataBehaviourType.ReadList,
                _ => throw new NotSupportedException()
            };
        }

        private static string GetMethodName(MethodDeclarationSyntax method)
            => method.Identifier.ValueText.Substring(0, method.Identifier.ValueText.Length - "Async".Length);
    }
}