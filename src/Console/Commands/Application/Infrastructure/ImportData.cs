using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.CLI.Commands.Application.Infrastructure
{
    internal class ImportData
    {
        public ImportData(string definition, string dataSource, List<IDictionary<string, object>> data)
        {
            Definition = definition;
            DataSource = dataSource;
            Data = data;
        }

        public string Definition { get; }
        public string DataSource { get; }
        public List<IDictionary<string, object>> Data { get; }

    }
}
