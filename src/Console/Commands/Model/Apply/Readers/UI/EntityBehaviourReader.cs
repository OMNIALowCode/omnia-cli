using Esprima;
using Esprima.Ast;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Commands.Model.Apply.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply.Readers.UI
{
    public class UIEntityBehaviourReader
    {
        public Form ExtractData(string text)
        {
            return new Form(ExtractEntityBehaviours(text));
        }

        private List<UIEntityBehaviour> ExtractEntityBehaviours(string script)
        {
            List<UIEntityBehaviour> entityBehaviours = new List<UIEntityBehaviour>();

            var parserOptions = new ParserOptions()
            {
                Loc = true,
                Range = true,
                Comment = true
            };

            var parser = new JavaScriptParser(script, parserOptions);

            var comments = GetFileComments(script, parserOptions);

            var parsedScript = parser.ParseScript();

            foreach (var @class in parsedScript.Body.Where(body => body is ClassDeclaration))
                entityBehaviours.AddRange(ExtractClassMethods(script, @class.As<ClassDeclaration>(), comments));

            return entityBehaviours;
        }

        private List<UIEntityBehaviour> ExtractClassMethods(string script, ClassDeclaration @class, List<Comment> comments)
        {
            List<UIEntityBehaviour> entityBehaviours = new List<UIEntityBehaviour>();

            foreach (var method in @class.Body.ChildNodes.OfType<MethodDefinition>())
            {
                var function = method.ChildNodes.OfType<FunctionExpression>().FirstOrDefault();

                string functionName = ((Esprima.Ast.Identifier)method.Key).Name;

                if (function != null)
                {
                    var entityBehaviour = MapFunction(functionName, script, function, comments);
                    if (IsValid(entityBehaviour))
                        entityBehaviours.Add(entityBehaviour);
                }
            }
            return entityBehaviours;

            static bool IsValid(UIEntityBehaviour behaviour)
                => behaviour != null && !string.IsNullOrEmpty(behaviour.Expression);
        }

        private static UIEntityBehaviour MapFunction(string functionName, string script, FunctionExpression function, List<Comment> comments)
        {
            var functionBody = function.Body;

            var blockStartLine = functionBody.Location.Start.Line;

            var (name, description) = UIMethodInfoExtension.GetJavascriptCommentInfo(comments, blockStartLine - 1, script);

            var functionSnippet = script[functionBody.Range.Start..functionBody.Range.End];

            var type = MapType(functionName);

            if (type.HasValue)
                return new UIEntityBehaviour
                {
                    Expression = UIMethodInfoExtension.ExtractExpression(functionSnippet),
                    Name = name ?? functionName,
                    Description = description,
                    Type = type.Value,
                    Attribute = GetAttribute(type.Value, functionName)
                };

            return null;
        }

        private List<Comment> GetFileComments(string script, ParserOptions parserOptions)
        {
            var scanner = new Scanner(script, parserOptions);

            Token token;

            var comments = new List<Comment>();
            do
            {
                comments.AddRange(scanner.ScanComments());
                token = scanner.Lex();
            } while (token.Type != TokenType.EOF);

            return comments;
        }

        private static UIEntityBehaviourType? MapType(string methodName)
        {
            return methodName switch
            {
                var initialize when initialize.Equals("initialize") => UIEntityBehaviourType.Initialize,
                var change when change.StartsWith("onChange_") => UIEntityBehaviourType.Action,

                var afterChange when afterChange.Equals("afterChange") => UIEntityBehaviourType.AfterChange,
                var beforeChange when beforeChange.Equals("beforeChange") => UIEntityBehaviourType.BeforeChange,
                var beforeSave when beforeSave.Equals("beforeSave") => UIEntityBehaviourType.BeforeSave,

                _ => null
            };
        }

        private static string GetAttribute(UIEntityBehaviourType type, string name)
        {
            return type switch
            {
                UIEntityBehaviourType.Action => name.Substring("onChange_".Length, name.Length -9),
                _ => null
            };
        }
    }
}
