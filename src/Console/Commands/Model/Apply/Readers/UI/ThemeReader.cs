using Omnia.CLI.Commands.Model.Apply.Data;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using System;

namespace Omnia.CLI.Commands.Model.Apply.Readers.UI
{
    public class ThemeReader
    {

        public Theme ExtractData(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            return new Theme
            {
                Expression = text
            };
        }
    }
}
