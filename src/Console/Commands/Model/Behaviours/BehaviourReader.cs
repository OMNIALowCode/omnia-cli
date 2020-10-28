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
                var behaviour when behaviour.Equals(BehaviourType.Formula) || behaviour.Equals(BehaviourType.AfterSave) => SyntaxFactory.Block(nodes.OfType<ExpressionStatementSyntax>()).GetText().ToString().Trim('{', '}') + SyntaxFactory.Block(nodes.OfType<ReturnStatementSyntax>()).GetText().ToString().Trim('{', '}'),
                _ => SyntaxFactory.Block(nodes.OfType<ExpressionStatementSyntax>()).GetText().ToString().Trim('{', '}'),
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

                var afterChange when afterChange.Equals("ExecuteAfterUpdate") => BehaviourType.AfterChange,
                var beforeChange when beforeChange.Equals("ExecuteBeforeUpdate") => BehaviourType.BeforeChange,
                var beforeSave when beforeSave.Equals("ExecuteBeforeSave") => BehaviourType.BeforeSave,
                var afterSave when afterSave.Equals("ExecuteAfterSave") => BehaviourType.AfterSave,

                _ => throw new NotSupportedException()
            };
        }
    }
}