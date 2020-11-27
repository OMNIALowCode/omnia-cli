using Omnia.CLI.Commands.Model.Apply.Data;
using System;

namespace Omnia.CLI.Commands.Model.Apply.Readers
{
    public class WebComponentReader
    {
        private const string CustomElementDefinition = "customElements.define('";

        public WebComponent ExtractData(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            return new WebComponent
            {
                Expression = text,
                CustomElement = ExtractCustomElement(text)
            };
        }

        private static string ExtractCustomElement(string text)
        {
            if (!text.Contains(CustomElementDefinition)) throw new ArgumentException("Custom Element definition is missing in the provided text.");

            var startOfCustomElementName = text.IndexOf(CustomElementDefinition, StringComparison.InvariantCultureIgnoreCase) + CustomElementDefinition.Length;
            var textAfterCustomeElementDefine = text.Substring(startOfCustomElementName);
            return textAfterCustomeElementDefine.Substring(0, textAfterCustomeElementDefine.IndexOf("'", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
