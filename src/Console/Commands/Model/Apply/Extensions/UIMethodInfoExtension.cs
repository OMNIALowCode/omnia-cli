using Esprima;
using Omnia.CLI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.CLI.Commands.Model.Apply.Extensions
{
    public static class UIMethodInfoExtension
    {
        public static string ExtractExpression(string method)
        {
            return WithoutLeadingAndTrailingBraces(method).Trim();

            static string WithoutLeadingAndTrailingBraces(string blockText)
                => blockText
                    .Substring(0, blockText.LastIndexOf('}'))
                      .Substring(blockText.IndexOf('{') + 1);
        }

        public static (string name, string description) GetJavascriptCommentInfo(List<Comment> comments, int beginLine, string script)
        {
            var comment = comments.FirstOrDefault(a => a.Loc.End.HasValue && a.Loc.End.Value.Line.Equals(beginLine));

            if (comment != null)
            {
                var commentSnippet = script[comment.Start..comment.End];

                return ParseJavascriptComment(commentSnippet);
            }
            return (null, null);
        }


        public static (string name, string description) ParseJavascriptComment(string comment)
        {
            var content = comment
                    .Split(Environment.NewLine)
                    .Select(WithoutSpaces)
                    .Select(WithoutComment)
                    .Select(WithoutSpaces)
                    .Where(line => !IsDelimiterTag(line) && !string.IsNullOrEmpty(line))
                    .ToArray();

            if (content.Length == 0) return (null, null);

            var name = content[0];
            var description = string.Join(Environment.NewLine, content.Skip(1));

            return (name, description);

            static string WithoutSpaces(string text)
                => text.Trim();

            static string WithoutComment(string text)
                => text.TrimStart("*");

            static bool IsDelimiterTag(string text)
                => text.Equals("/**") || text.Equals("/");

        }
    }
}
