using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using ShellProgressBar;
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
        private List<IDictionary<String, object>> lines = new List<IDictionary<string, object>>();
        private List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)> data = new List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)>();

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

            ReadExcelAsync(this.Path);

            //var data = new List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)>();

            //{ {"_code", "A" } }
            //    }));

            await ProcessDefinitions(this.data);

            Console.WriteLine($"Successfully imported data to tenant \"{Tenant}\".");
            return (int)StatusCodes.Success;
        }

        private async void ReadExcelAsync(string path)
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

                this.data.Add((entityName, "default", new List<IDictionary<string, object>>(lines)));
                lines.Clear();
            }
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

        private async Task ProcessDefinitions(List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)> data)
        {
            using (var progressBar = new ProgressBar(data.Count, "Processing file..."))
            {
                foreach (var (Definition, DataSource, Data) in data)
                {
                    await CreateEntities(progressBar, _httpClient, Tenant, Environment, Definition, DataSource, Data);
                    progressBar.Tick();
                }
            }
        }

        private static async Task CreateEntities(ProgressBar progressBar, HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IList<IDictionary<string, object>> data)
        {
            using (var child = progressBar.Spawn(data.Count, "Processing entity..."))
            {
                foreach (var entity in data)
                    _ = await CreateEntity(httpClient, tenantCode, environmentCode, definition, dataSource, entity);
                child.Tick();
            }
        }

        private static async Task<int> CreateEntity(HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IDictionary<string, object> data)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"/api/v1/{tenantCode}/{environmentCode}/application/{definition}/{dataSource}", data);
                if (response.IsSuccessStatusCode)
                {
                    return (int)StatusCodes.Success;
                }

                var apiError = await GetErrorFromApiResponse(response);

                Console.WriteLine($"{apiError.Code}: {apiError.Message}");

                return (int)StatusCodes.InvalidOperation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => response.Content.ReadAsJsonAsync<ApiError>();
    }
}
