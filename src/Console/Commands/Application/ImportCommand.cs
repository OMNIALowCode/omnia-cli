using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Omnia.CLI.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Application
{
    [Command(Name = "import", Description = "Import application data.")]
    [HelpOption("-h|--help")]
    public class ImportCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        private List<string> headers = new List<string>();
        private List<Dictionary<String, object>> lines = new List<Dictionary<string, object>>();

        public ImportCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }

        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant to export.")]
        public string Tenant { get; set; }

        [Option("--environment", CommandOptionType.SingleValue, Description = "Environment to export.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path to the file.")]
        public string Path { get; set; }

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrEmpty(Path))
            {
                Console.WriteLine($"{nameof(Path)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (!File.Exists(Path))
            {
                Console.WriteLine($"The value of --path parameters \"{Path}\" is not a valid file.");
                return (int)StatusCodes.InvalidArgument;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _httpClient.WithSubscription(sourceSettings);

            /* TODO
                    GET File
                    Reed Sheets
                    Get Entity
                    Read Headers
                    Read Rows
                    Compose Json
                    Call API
            */
            Console.WriteLine($"Successfully imported data to tenant \"{Tenant}\".");
            return (int)StatusCodes.Success;
        }

        private void ReadExcel(string path)
        {
            var workbook = new XSSFWorkbook(path);

            Console.WriteLine("NumberOfSheets:{0}", workbook.NumberOfSheets);

            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                var sheet = workbook.GetSheetAt(s);

                string entityName = sheet.SheetName;

                Console.WriteLine("entityName:{0}", entityName);

                Console.WriteLine("PhysicalNumberOfRows:{0}", sheet.PhysicalNumberOfRows);

                for (int rownum = 0; rownum < sheet.PhysicalNumberOfRows; rownum++)
                {
                    var row = sheet.GetRow(rownum);

                    if (rownum == 0)
                    {
                        GetHeaders(row);
                    }
                    else
                    {
                        GetLines(row);
                    }
                }
            }

            string json = JsonConvert.SerializeObject(lines);

            Console.WriteLine("Json:{0}", json);
        }

        private void GetLines(IRow row)
        {
            Dictionary<string, object> line = new Dictionary<string, object>();
            for (int cellnum = 0; cellnum < row.Cells.Count; cellnum++)
            {
                line.Add(headers[cellnum], row.Cells[cellnum].ToString());
            }
            lines.Add(line);
        }

        private void GetHeaders(IRow row)
        {
            foreach (var item in row)
            {
                headers.Add(item.StringCellValue);
            }
        }

        private class ApiError
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }
    }
}
