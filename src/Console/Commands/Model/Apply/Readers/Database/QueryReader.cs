using Omnia.CLI.Commands.Model.Apply.Data.Database;
using System;

namespace Omnia.CLI.Commands.Model.Apply.Readers.Database
{
    public class QueryReader
    {

        public Query ExtractData(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            return new Query
            {
                Expression = text
            };
        }
    }
}
