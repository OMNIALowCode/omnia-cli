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
            var statement = method.DescendantNodes()
                .OfType<ExpressionStatementSyntax>().FirstOrDefault();

            return new Behaviour
            {
                Expression = statement?.GetText().ToString(),
                Name = method.Identifier.ValueText,
                Type = MapType(method)
            };
        }

        private static BehaviourType MapType(MethodDeclarationSyntax method)
        {
            return method.Identifier.ValueText switch
            {

                var initialize when initialize.Equals("ExecuteInitialize") => BehaviourType.Initialize,
                var change when change.StartsWith("On") && change.EndsWith("PropertyChange") => BehaviourType.Action,
                _ => BehaviourType.AfterChange
                //_ => throw new NotImplementedException()
            };
        }
    }
}