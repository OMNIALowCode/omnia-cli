﻿using McMaster.Extensions.CommandLineUtils;
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
using System.Linq;
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
        private readonly List<IDictionary<String, object>> _lines = new List<IDictionary<string, object>>();
        private readonly List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)> _data = new List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)>();

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


            var success = await ProcessDefinitions(this._data);

            if (!success) return (int)StatusCodes.UnknownError;

            Console.WriteLine($"Successfully imported data to tenant \"{Tenant}\".");
            return (int)StatusCodes.Success;
        }

        private void ReadExcelAsync(string path)
        {
            var workbook = new XSSFWorkbook(path);

            Console.WriteLine("NumberOfSheets:{0}", workbook.NumberOfSheets);

            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                var sheet = workbook.GetSheetAt(s);

                var namingParts = GetSheetNameWithoutNamingKey(sheet.SheetName).Split('.');
                var entityName = namingParts[0];
                var dataSource = namingParts.Length > 1 ? namingParts[1] : "default";

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

                this._data.Add((entityName, dataSource, new List<IDictionary<string, object>>(_lines)));
                _lines.Clear();
            }

            string GetSheetNameWithoutNamingKey(string sheetName)
                => sheetName.Split('-')[0];
        }

        private void GetLines(IRow row)
        {
            Dictionary<string, object> line = new Dictionary<string, object>();
            for (int cellnum = 0; cellnum < row.Cells.Count; cellnum++)
            {
                line.Add(headers[cellnum], row.Cells[cellnum].ToString());
            }
            _lines.Add(line);
        }

        private void GetHeaders(IRow row)
        {
            foreach (var item in row)
            {
                headers.Add(item.StringCellValue);
            }
        }

        private async Task<bool> ProcessDefinitions(List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)> data)
        {
            var failed = new List<string>();
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Cyan,
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var progressBar = new ProgressBar(data.Count, "Processing file...", options))
            {
                foreach (var (Definition, DataSource, Data) in data)
                {
                    var (Success, Messages) = await CreateEntities(progressBar, _httpClient, Tenant, Environment, Definition, DataSource, Data);
                    progressBar.Tick();

                    if (Success)
                        continue;

                    failed.AddRange(Messages);
                }
            }

            Console.WriteLine($"----- Failed: {failed.Count()} -----");
            foreach (var message in failed)
                Console.WriteLine(message);

            return !failed.Any();
        }

        private static async Task<(bool Success, string[] Messages)> CreateEntities(ProgressBar progressBar, HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IList<IDictionary<string, object>> data)
        {
            var failedEntities = new List<string>();

            using (var child = progressBar.Spawn(data.Count, "Processing entity..."))
            {
                foreach (var entity in data)
                {
                    var result = await CreateEntity(httpClient, tenantCode, environmentCode, definition, dataSource, entity);

                    child.Tick(message: result == (int)StatusCodes.Success ? null : $"Error creating entity for {dataSource} {definition}");
                    if (result != (int)StatusCodes.Success)
                    {
                        child.ForegroundColor = ConsoleColor.DarkRed;
                        failedEntities.Add($"{dataSource}.{definition}: {string.Join(";", entity.Select(c => c.Value))}");
                    }
                }
            }

            return (!failedEntities.Any(), failedEntities.ToArray());
        }

        private static async Task<int> CreateEntity(HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IDictionary<string, object> data)
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

        private static Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => response.Content.ReadAsJsonAsync<ApiError>();
    }
}
