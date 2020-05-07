using System.Collections.Generic;

namespace Omnia.CLI.Commands.Application.Infrastructure
{
    internal class ImportData
    {
        public ImportData(string definition, string dataSource, List<(int RowNum, IDictionary<string, object> Data)> data)
        {
            Definition = definition;
            DataSource = dataSource;
            Data = data;
        }

        public string Definition { get; }
        public string DataSource { get; }
        public List<(int RowNum, IDictionary<string, object> Data)> Data { get; }
    }
}
