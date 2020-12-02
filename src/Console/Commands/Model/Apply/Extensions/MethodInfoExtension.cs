using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Extensions;
using System;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply.Extensions
{
    public static class MethodInfoExtension
    {
        public static string ExtractExpression(this MethodDeclarationSyntax method)
        {
            var blockText = method.Body?.ToFullString();
            return WithoutLeadingAndTrailingBraces(blockText).Trim();

            static string WithoutLeadingAndTrailingBraces(string blockText)
                => blockText
                    .Substring(0, blockText.LastIndexOf('}'))
                      .Substring(blockText.IndexOf('{') + 1);
        }

        public static (string name, string description) ExtractDataFromComment(this MethodDeclarationSyntax method)
        {
            var comments = method.GetLeadingTrivia();
            foreach (var comment in comments)
            {
                var xml = comment.GetStructure();

                if (xml == null) continue;

                return ParseXmlComment(xml.ToFullString());
            }

            return (null, null);
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
