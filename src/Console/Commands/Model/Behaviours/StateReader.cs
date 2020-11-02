using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    public class StateReader
    {
        public IList<State> ExtractMethods(string text)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();

            return ExtractStates(root);
        }

        private IList<State> ExtractStates(CompilationUnitSyntax root)
        {

            var methods = root.DescendantNodes(null, false)
                .OfType<MethodDeclarationSyntax>()
                .ToList();

            foreach(var method in methods)
            {
                string type ;
                string expression;
                string state;

                var methodName =  method.Identifier.ToFullString();


                if(methodName.StartsWith("AssignTo_"))
                {
                    type = "Assign";
                    expression = ExtractExpression(method);
                    state = methodName.Substring("AssignTo_".Length);
                }else if(methodName.StartsWith("EvaluateStateTransition_"))
                {
                    type = "Transition";
                    expression = ExtractExpression(method);
                    state = methodName.Substring("EvaluateStateTransition_".Length);
                }
                else if (methodName.StartsWith("On") && methodName.EndsWith("In"))
                {
                    //
                }
                else if (methodName.StartsWith("On") && methodName.EndsWith("Out"))
                {
                    //
                }

            }



            return null;

        }

        private static string ExtractExpression(MethodDeclarationSyntax method)
        {
            var nodes = method.DescendantNodes();

            var blockText = nodes.OfType<BlockSyntax>().SingleOrDefault()?.ToFullString();
            return RemoveWithoutLeadingAndTrailingBraces(blockText).Trim();

            static string RemoveWithoutLeadingAndTrailingBraces(string blockText)
                => blockText
                    .Substring(0, blockText.LastIndexOf('}'))
                      .Substring(blockText.IndexOf('{') + 1);

        }

    }
}