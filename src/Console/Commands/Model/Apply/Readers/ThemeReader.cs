using Omnia.CLI.Commands.Model.Apply.Data;
using System;

namespace Omnia.CLI.Commands.Model.Apply.Readers
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
