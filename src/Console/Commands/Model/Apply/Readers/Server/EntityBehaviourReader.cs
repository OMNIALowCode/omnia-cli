using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Apply.Data;
using Omnia.CLI.Commands.Model.Apply.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Omnia.CLI.Commands.Model.Apply.Readers.Server
{
    public class EntityBehaviourReader
    {
        private const string BehaviourNamespacePrefix = "Omnia.Behaviours.";
        private static readonly string[] DefaultUsings = {
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
            var namespaceDeclaration = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclaration.Single().Name.ToString();
        }

        private static IList<EntityBehaviour> ExtractMethods(CompilationUnitSyntax root)
        {
            return root.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .Select(MapMethod)
                            .Where(HasExpression)
                            .ToList();

            static bool HasExpression(EntityBehaviour behaviour)
                => !string.IsNullOrEmpty(behaviour.Expression);
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

        private static EntityBehaviour MapMethod(MethodDeclarationSyntax method)
        {
            var (name, description) = method.ExtractDataFromComment();

            var type = MapType(method);
            return new EntityBehaviour
            {
                Expression = method.ExtractExpression(),
                Name = name ?? method.Identifier.ValueText,
                Description = description,
                Type = type,
                Attribute = GetAttribute(type, method.Identifier.ValueText)
            };
        }

        private static string GetAttribute(EntityBehaviourType type, string name)
        {
            return type switch
            {
                EntityBehaviourType.Action => name.Substring("On".Length, name.Length - "PropertyChange".Length - 2),
                EntityBehaviourType.Formula => name.Substring("Get".Length, name.Length - 3),
                EntityBehaviourType.BeforeCollectionEntityInitialize => name.Substring("OnBefore".Length, name.Length - "EntityInitialize".Length - 8),
                _ => null,
            };
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
    }
}