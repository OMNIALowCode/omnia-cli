using Esprima;
using Esprima.Ast;
using Omnia.CLI.Commands.Model.Apply.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.CLI.Commands.Model.Apply.Readers
{
    public class UIEntityBehaviourReader
    {
        public void ExtractData(string text)
        {
            ExtractClasses(text);
        }

        private void ExtractClasses(string script)
        {
            var parser = new JavaScriptParser(script, new ParserOptions()
            {
                Range = true,
                Comment = true
            });

            var parsedScript = parser.ParseScript();

            foreach (var @class in parsedScript.Body.Where(b => b is ClassDeclaration))
            {
                var classDeclaration = @class.As<ClassDeclaration>();

                ExtractMethods(script, classDeclaration);
            }
        }

        private List<EntityBehaviour> ExtractMethods(string script, ClassDeclaration @class)
        {

            return @class.Body.ChildNodes
                .OfType<MethodDefinition>()
                .Select(MapMethod)
                .Where(HasExpression)
                .ToList();

            static bool HasExpression(EntityBehaviour behaviour)
                => !string.IsNullOrEmpty(behaviour.Expression);

            var functions = @class.Body.ChildNodes.OfType<MethodDefinition>();
            foreach (var function in functions)
            {
                var methodName = ((Esprima.Ast.Identifier)function.Key).Name;
                
                var blocks = function.ChildNodes.OfType<FunctionExpression>();

                foreach (var block in blocks)
                {
                    var blockBody = block.Body;

                    var snippet = script[blockBody.Range.Start..blockBody.Range.End];
                }
            }
        }

        private static EntityBehaviour MapMethod(MethodDefinition method)
        {
            //var (name, description) = method.ExtractDataFromComment();

            //var type = MapType(method);
            return new EntityBehaviour
            {
                //Expression = method.ExtractExpression(),
                //Name = name ?? method.Identifier.ValueText,
                //Description = description,
                //Type = type,
                //Attribute = GetAttribute(type, method.Identifier.ValueText)
            };
        }
    }
}
