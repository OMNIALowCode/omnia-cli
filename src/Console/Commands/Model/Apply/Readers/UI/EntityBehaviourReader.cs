using Esprima;
using Esprima.Ast;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Commands.Model.Apply.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply.Readers.UI
{
    public class UIEntityBehaviourReader
    {
        public UIEntity ExtractData(string text)
        {
            return new UIEntity(ExtractEntityBehaviours(text));
        }

        private List<UIBehaviour> ExtractEntityBehaviours(string script)
        {
            var parserOptions = new ParserOptions()
            {
                Loc = true,
                Range = true,
                Comment = true
            };

            var parser = new JavaScriptParser(script, parserOptions);

            var comments = GetFileComments(script, parserOptions);

            var parsedScript = parser.ParseScript();

            var entityBehaviours = new List<UIBehaviour>();
            foreach (var @class in parsedScript.Body.Where(body => body is ClassDeclaration))
                entityBehaviours.AddRange(ExtractClassMethods(script, @class.As<ClassDeclaration>(), comments));

            return entityBehaviours;
        }

        private List<UIBehaviour> ExtractClassMethods(string script, ClassDeclaration @class, List<Comment> comments)
        {
            var entityBehaviours = new List<UIBehaviour>();

            var className = @class.Id?.Name;

            foreach (var method in @class.Body.ChildNodes.OfType<MethodDefinition>())
            {
                var function = method.ChildNodes.OfType<FunctionExpression>().SingleOrDefault();

                if (function == null) continue;

                var functionName = ((Identifier)method.Key).Name;

                var entityBehaviour = MapFunction(className, functionName, script, function, comments);
                if (IsValidBehaviour(entityBehaviour))
                    entityBehaviours.Add(entityBehaviour);
            }
            return entityBehaviours;

            static bool IsValidBehaviour(UIBehaviour behaviour)
                => behaviour != null && !string.IsNullOrEmpty(behaviour.Expression);
        }

        private static UIBehaviour MapFunction(string className, string functionName, string script, FunctionExpression function, List<Comment> comments)
        {
            var blockStartLine = function.Location.Start.Line;

            var (name, description) = UIMethodInfoExtension.GetJavaScriptCommentInfo(comments, blockStartLine - 1, script, functionName);

            var functionSnippet = script[function.Body.Range.Start..function.Body.Range.End];

            var type = MapType(functionName);

            if (!type.HasValue)
                return null;

            return new UIBehaviour
            {
                Expression = UIMethodInfoExtension.ExtractExpression(functionSnippet),
                Name = GetFunctionName(name, className, functionName, type.Value.Equals(UIBehaviourType.Change)),
                Description = description,
                Type = type.Value,
                Element = GetElement(type.Value, functionName),
                Definition = className
            };
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

        private static UIBehaviourType? MapType(string methodName)
        {
            return methodName switch
            {
                var initialize when initialize.Equals("onInitialize") => UIBehaviourType.Initialize,
                var change when change.StartsWith("onChange_") => UIBehaviourType.Change,

                var afterChange when afterChange.Equals("onAfterChange") => UIBehaviourType.AfterChange,
                var beforeChange when beforeChange.Equals("onBeforeChange") => UIBehaviourType.BeforeChange,
                var beforeSave when beforeSave.Equals("onBeforeSave") => UIBehaviourType.BeforeSave,

                var select when select.StartsWith("onSelect_") => UIBehaviourType.Select,

                _ => null
            };
        }

        private static string GetElement(UIBehaviourType type, string name)
        {
            return type switch
            {
                UIBehaviourType.Change => $"{name.Substring("onChange_".Length, name.Length - 9)}",
                UIBehaviourType.Select => $"{name.Substring("onSelect_".Length, name.Length - 9)}",
                _ => null
            };
        }

        private static string GetFunctionName(string name, string className, string functionName, bool isAction)
        {
            return name ?? (isAction ? string.Format("{0}_{1}", className, functionName) : functionName);
        }
    }
}
