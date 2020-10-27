using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    public class BehaviourReader
    {

        public IList<Behaviour> ExtractMethods(string text)
        {

            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return root.DescendantNodes(null, false)
                .OfType<MethodDeclarationSyntax>()
                .Select(MapMethod)
                .Where(HasExpression)
                .ToList();

            static bool HasExpression(Behaviour m)
                => !string.IsNullOrEmpty(m.Expression);
        }

        private static Behaviour MapMethod(MethodDeclarationSyntax method)
        {
            var methodType = MapType(method);
            var expression = ExtractExpression(method, methodType);

            return new Behaviour
            {
                Expression = expression,
                Name = method.Identifier.ValueText,
                Type = methodType
            };
        }

        private static string ExtractExpression(MethodDeclarationSyntax method, BehaviourType type)
        {
            var nodes = method.DescendantNodes();

            return type switch
            {
                BehaviourType.Formula => nodes.OfType<ReturnStatementSyntax>().FirstOrDefault()?.GetText().ToString(),
                _ => nodes.OfType<ExpressionStatementSyntax>().FirstOrDefault()?.GetText().ToString(),
            };
        }

        private static BehaviourType MapType(MethodDeclarationSyntax method)
        {
            return method.Identifier.ValueText switch
            {

                var initialize when initialize.Equals("ExecuteInitialize") => BehaviourType.Initialize,
                var change when change.StartsWith("On") && change.EndsWith("PropertyChange") => BehaviourType.Action,
                var formula when formula.StartsWith("Get") => BehaviourType.Formula,
                var beforeCollectionEntityInitialize when beforeCollectionEntityInitialize.StartsWith("Before") && beforeCollectionEntityInitialize.EndsWith("EntityInitialize") => BehaviourType.BeforeCollectionEntityInitialize,

                _ => BehaviourType.AfterChange,
            };
        }
    }
}