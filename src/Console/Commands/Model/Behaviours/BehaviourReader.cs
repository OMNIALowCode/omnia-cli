using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using Omnia.CLI.Extensions;
namespace Omnia.CLI.Commands.Model.Behaviours
{
    public class BehaviourReader
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
            "Omnia.Libraries.Infrastructure.Connector",
            "Omnia.Libraries.Infrastructure.Connector.Client",
            "System.Net.Http.Formatting",
            "System.Net.Http",
            "Microsoft.Extensions.DependencyInjection",
            "Omnia.Libraries.Infrastructure.Behaviours",
            "Omnia.Libraries.Infrastructure.Behaviours.Action",
        };

        public Entity ExtractData(string text)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return new Entity(
                ExtractNamespace(root),
                ExtractMethods(root),
                null,
                ExtractUsings(root));
        }

        private static string ExtractNamespace(CompilationUnitSyntax root)
        {
            var namespaceDeclaration = root.DescendantNodes(null, false)
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclaration.Single().Name.ToString();
        }

        private static IList<EntityBehaviour> ExtractMethods(CompilationUnitSyntax root)
        {
            return root.DescendantNodes(null, false)
                            .OfType<MethodDeclarationSyntax>()
                            .Select(MapMethod)
                            .Where(HasExpression)
                            .ToList();

            static bool HasExpression(EntityBehaviour m)
                => !string.IsNullOrEmpty(m.Expression);
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

        private static EntityBehaviour MapMethod(MethodDeclarationSyntax method)
        {
            var methodType = MapType(method);
            var expression = ExtractExpression(method);

            string name = null;
            string description = null;

            var comments = method.GetLeadingTrivia();
            foreach (var comment in comments)
            {
                var xml = comment.GetStructure();

                if (xml == null) continue;

                (name, description) = ParseXmlComment(xml.ToFullString());
            }

            return new EntityBehaviour
            {
                Expression = expression,
                Name = name ?? method.Identifier.ValueText,
                Description = description,
                Type = methodType
            };
        }

        private static string ExtractExpression(MethodDeclarationSyntax method)
        {
            
            var blockText =  method.Body?.ToFullString();
            return WithoutLeadingAndTrailingBraces(blockText).Trim();

            static string WithoutLeadingAndTrailingBraces(string blockText)
                => blockText
                    .Substring(0, blockText.LastIndexOf('}'))
                      .Substring(blockText.IndexOf('{') + 1);  
        }

        private static EntityBehaviourType MapType(MethodDeclarationSyntax method)
        {
            return method.Identifier.ValueText switch
            {
                var initialize when initialize.Equals("OnInitialize") => EntityBehaviourType.Initialize,
                var change when change.StartsWith("On") && change.EndsWith("PropertyChange") => EntityBehaviourType.Action,
                var formula when formula.StartsWith("Get") => EntityBehaviourType.Formula,
                var beforeCollectionEntityInitialize when beforeCollectionEntityInitialize.StartsWith("OnBefore") && beforeCollectionEntityInitialize.EndsWith("EntityInitialize") => EntityBehaviourType.BeforeCollectionEntityInitialize,

                var afterChange when afterChange.Equals("OnAfterUpdate") => EntityBehaviourType.AfterChange,
                var beforeChange when beforeChange.Equals("OnBeforeUpdate") => EntityBehaviourType.BeforeChange,
                var beforeSave when beforeSave.Equals("OnBeforeSave") => EntityBehaviourType.BeforeSave,
                var afterSave when afterSave.Equals("OnAfterSave") => EntityBehaviourType.AfterSave,

                _ => throw new NotSupportedException()
            };
        }

        private static (string name, string description) ParseXmlComment(string comment)
        {
            var content = comment
                    .Split(Environment.NewLine)
                    .Select(WithoutSpaces)
                    .Select(WithoutComment)
                    .Select(WithoutSpaces)
                    .Where(line => !IsSummaryTag(line) && !string.IsNullOrEmpty(line))
                    .ToArray();

            if (content.Length == 0) return (null, null);

            var name = content[0];
            var description = string.Join(Environment.NewLine, content.Skip(1));

            return (name, description);

            static string WithoutSpaces(string text)
                => text.Trim();

            static string WithoutComment(string text)
                => text.TrimStart("///");

            static bool IsSummaryTag(string text)
                => text.Equals("<summary>") || text.Equals("</summary>");
        }
    }
}