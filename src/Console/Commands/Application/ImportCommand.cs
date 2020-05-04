using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Application
{
    [Command(Name = "import", Description = "Import application data.")]
    [HelpOption("-h|--help")]
    public class ImportCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly List<string> _headers = new List<string>();
        private readonly List<IDictionary<string, object>> _lines = new List<IDictionary<string, object>>();
        private readonly List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)> _data = new List<(string Definition, string DataSource, List<IDictionary<string, object>> Data)>();
        private readonly List<string> _sheets = new List<string>();
        private readonly List<string> _sheetsProcessed = new List<string>();

        private readonly Dictionary<string, List<ImportCollection>> _dictionaryCollections = new Dictionary<string, List<ImportCollection>>();

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

            ReadExcelAsync(this.Path);

            var success = await ProcessDefinitions(this._data);

            if (!success) return (int)StatusCodes.UnknownError;

            Console.WriteLine($"Successfully imported data to tenant \"{Tenant}\".");
            return (int)StatusCodes.Success;
        }

        private void ReadExcelAsync(string path)
        {
            var workbook = new XSSFWorkbook(path);

            LoadSheets(workbook);

            foreach (var sheet in this._sheets)
            {
                if (this._sheetsProcessed.Contains(sheet))
                {
                    continue;
                }

                var activeWorksheet = workbook.GetSheetAt(workbook.GetSheetIndex(sheet));

                var namingParts = GetSheetNameWithoutNamingKey(activeWorksheet.SheetName).Split('.');
                var entityName = namingParts[0];
                var dataSource = namingParts.Length > 1 ? namingParts[1] : "default";

                if (_sheets.Any(s => s.StartsWith($"{sheet}.")))
                {
                    ProcessEntityWithCollections(workbook, sheet, entityName, dataSource);
                }
                else
                {
                    ProcessSimpleEntity(activeWorksheet, entityName, dataSource);
                }
            }

            string GetSheetNameWithoutNamingKey(string sheetName)
               => sheetName.Split('-')[0];
        }

        private void ProcessSimpleEntity(ISheet activeWorksheet, string entityName, string dataSource)
        {
            for (var rowNum = 0; rowNum < activeWorksheet.PhysicalNumberOfRows; rowNum++)
            {
                var row = activeWorksheet.GetRow(rowNum);

                if (row == null) continue;
                
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                if (rowNum == 0)
                {
                    GetHeaders(row);
                }
                else
                {
                    GetLines(row);
                }
            }

            this._data.Add((entityName, dataSource, new List<IDictionary<string, object>>(_lines)));
            ResetData();
        }

        private void ProcessEntityWithCollections(XSSFWorkbook workbook, string sheet, string entityName, string dataSource)
        {
            foreach (var item in new List<string>(this._sheets.Where(s => s.StartsWith(sheet))))
            {
                var importCollections = new List<ImportCollection>();

                var activeWorksheet = workbook.GetSheetAt(workbook.GetSheetIndex(item));

                for (var rowNum = 0; rowNum < activeWorksheet.PhysicalNumberOfRows; rowNum++)
                {
                    var row = activeWorksheet.GetRow(rowNum);

                    if (row == null) continue;
                    
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                    if (rowNum == 0)
                    {
                        GetHeaders(row);
                    }
                    else
                    {
                        var importCollection = new ImportCollection();
                        GetLinesCollection(row, importCollection);
                        importCollections.Add(importCollection);
                    }
                }

                _dictionaryCollections.Add(item, importCollections);
                _headers.Clear();
                this._sheetsProcessed.Add(item);
            }

            ProcessHierarchy(this._dictionaryCollections.Keys.Last());

            var collection = PrepareCollection();
            this._data.Add((entityName, dataSource, new List<IDictionary<string, object>>(collection)));

            ResetData();
        }

        private IEnumerable<IDictionary<string, object>> PrepareCollection()
        {
            var collection = new List<IDictionary<string, object>>();

            foreach (var item in this._dictionaryCollections.FirstOrDefault().Value)
            {
                collection.Add(item.Data);
            }

            return collection;
        }

        private void ResetData()
        {
            _lines.Clear();
            _headers.Clear();
            this._dictionaryCollections.Clear();
        }

        private void LoadSheets(XSSFWorkbook workbook)
        {
            for (var sheetNumber = 0; sheetNumber < workbook.NumberOfSheets; sheetNumber++)
            {
                _sheets.Add(workbook.GetSheetAt(sheetNumber).SheetName);
            }
        }

        private void ProcessHierarchy(string entity)
        {
            var level = entity.Count(c => c.Equals('.'));
            if (level == 0) return;
            
            var importData = this._dictionaryCollections[entity];

            var importCollection = _dictionaryCollections.FirstOrDefault(s => s.Key.Equals(entity.Substring(0, entity.LastIndexOf('.')))).Value;
            foreach (var parent in importCollection)
            {
                var childData = importData.Where(i => i.ParentId == parent.Id);
                var field = entity.Split(".")[level];
                parent.Data.Add(field, childData.Select(s => s.Data));
            }
            _dictionaryCollections.Remove(entity);
            ProcessHierarchy(this._dictionaryCollections.Keys.Last());
        }

        private void GetLinesCollection(IRow row, ImportCollection collection)
        {
            collection.Data = new Dictionary<string, object>();
            if (row == null) return;

            for (var cellNum = 0; cellNum < row.LastCellNum; cellNum++)
            {
                LoadDataToCollection(row, collection, cellNum);
            }
        }

        private void LoadDataToCollection(IRow row, ImportCollection collection, int cellNum)
        {
            var cell = row.GetCell(cellNum, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            var header = _headers.Count > cellNum ? _headers[cellNum] : string.Empty;

            if (header.Equals("#ID"))
                collection.Id = cell.ToString();
            else if (header.Equals("ParentID"))
                collection.ParentId = cell.ToString();
            else if (string.IsNullOrEmpty(header))
                return;

            switch (cell.CellType)
            {
                case CellType.String:
                    collection.Data.Add(header, cell.StringCellValue);
                    break;

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        collection.Data.Add(header, cell.DateCellValue);
                    else if (cell.CellStyle.DataFormat >= 164 && DateUtil.IsValidExcelDate(cell.NumericCellValue))
                        collection.Data.Add(header, cell.DateCellValue);
                    else
                        collection.Data.Add(header, cell.NumericCellValue);
                    break;

                case CellType.Boolean:
                    collection.Data.Add(header, cell.BooleanCellValue);
                    break;

                case CellType.Formula:
                    collection.Data.Add(header, cell.CellFormula);
                    break;

                case CellType.Error:
                    collection.Data.Add(header, FormulaError.ForInt(cell.ErrorCellValue).String);
                    break;

                case CellType.Blank:
                    collection.Data.Add(header, string.Empty);
                    break;

                case CellType.Unknown:
                    break;
                
                default:
                    throw new InvalidDataException($"Unknown Cell Type: {cell.CellType}");
            }

        }

        private void LoadDataToLine(IRow row, IDictionary<string, object> line, int cellNum)
        {
            var cell = row.GetCell(cellNum, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            var header = _headers.Count > cellNum ? _headers[cellNum] : string.Empty;
            if (string.IsNullOrEmpty(header))
                return;

            switch (cell.CellType)
            {
                case CellType.String:
                    line.Add(header, cell.StringCellValue);
                    break;

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        line.Add(header, cell.DateCellValue);
                    else if (cell.CellStyle.DataFormat >= 164 && DateUtil.IsValidExcelDate(cell.NumericCellValue))
                        line.Add(header, cell.DateCellValue);
                    else
                        line.Add(header, cell.NumericCellValue);
                    break;

                case CellType.Boolean:
                    line.Add(_headers[cellNum], cell.BooleanCellValue);
                    break;

                case CellType.Formula:
                    line.Add(_headers[cellNum], cell.CellFormula);
                    break;

                case CellType.Error:
                    line.Add(_headers[cellNum], FormulaError.ForInt(cell.ErrorCellValue).String);
                    break;

                case CellType.Blank:
                    line.Add(_headers[cellNum], string.Empty);
                    break;

                case CellType.Unknown:
                    break;

                default:
                    throw new InvalidDataException($"Unknown Cell Type: {cell.CellType}");
            }
        }


        private void GetLines(IRow row)
        {
            var line = new Dictionary<string, object>();
            if (row == null) return;
            for (var cellNum = 0; cellNum < row.LastCellNum; cellNum++)
            {
                LoadDataToLine(row, line, cellNum);
            }
            _lines.Add(line);
        }

        private void GetHeaders(IRow row)
        {
            foreach (var item in row)
            {
                _headers.Add(item.StringCellValue);
            }
        }

        private async Task<bool> ProcessDefinitions(IReadOnlyCollection<(string Definition, string DataSource, List<IDictionary<string, object>> Data)> data)
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
                foreach (var (definition, dataSource, dataEntries) in data)
                {
                    var (success, messages) = await CreateEntities(progressBar, _httpClient, Tenant, Environment, definition, dataSource, dataEntries);
                    progressBar.Tick();

                    if (success)
                        continue;

                    failed.AddRange(messages);
                }
            }

            Console.WriteLine($"----- Failed: {failed.Count} -----");
            foreach (var message in failed)
                Console.WriteLine(message);

            return !failed.Any();
        }

        private static async Task<(bool Success, string[] Messages)> CreateEntities(ProgressBar progressBar, HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            ICollection<IDictionary<string, object>> data)
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
