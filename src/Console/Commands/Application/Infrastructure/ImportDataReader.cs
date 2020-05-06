using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Omnia.CLI.Commands.Application.Infrastructure
{
    internal class ImportDataReader
    {
        private const string Id = "#ID";
        private const string ParentId = "#ParentID";
        private readonly List<string> _headers = new List<string>();
        private readonly List<(int RowNum, IDictionary<string, object> Data)> _lines = new List<(int RowNum, IDictionary<string, object> Data)>();
        private readonly List<ImportData> _data = new List<ImportData>();
        private readonly List<(string Sheet, string Entity, string DataSource)> _sheets = new List<(string Sheet, string Entity, string DataSource)>();
        private readonly List<string> _sheetsProcessed = new List<string>();

        private readonly Dictionary<string, List<NestedCollection>> _dictionaryCollections = new Dictionary<string, List<NestedCollection>>();

        public IList<ImportData> ReadExcel(string path)
        {
            var workbook = new XSSFWorkbook(path);

            LoadSheetNames(workbook);

            ScrollSheets(workbook);

            workbook.Close();

            return _data;
        }

        private void ScrollSheets(XSSFWorkbook workbook)
        {
            foreach (var sheet in _sheets)
            {
                if (_sheetsProcessed.Contains(sheet.Sheet))
                {
                    continue;
                }

                var activeWorksheet = workbook.GetSheetAt(workbook.GetSheetIndex(sheet.Sheet));
                var entityName = sheet.Entity;
                var dataSource = string.IsNullOrEmpty(sheet.DataSource) ? "Default" : sheet.DataSource;  //GetDataSource(namingParts);

                List<(int RowNum, IDictionary<string, object> Data)> lines;

                if (_sheets.Any(s => s.Entity.StartsWith($"{entityName}.")))
                {
                    lines = ProcessCollectionSheet(workbook, entityName);
                }
                else
                {
                    lines = ProcessSimpleSheet(activeWorksheet);
                }

                _data.Add(new ImportData(entityName, dataSource, lines));

                ResetAllData();
            }
        }

        private List<(int RowNum, IDictionary<string, object> Data)> ProcessSimpleSheet(ISheet activeWorksheet)
        {
            if (activeWorksheet.PhysicalNumberOfRows == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The sheet {activeWorksheet.SheetName} is empty");
                Console.ResetColor();
            }

            for (var rowNum = 0; rowNum < activeWorksheet.PhysicalNumberOfRows; rowNum++)
            {
                var row = activeWorksheet.GetRow(rowNum);

                if (row == null) continue;

                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                switch (rowNum)
                {
                    case 0:
                        ProcessHeaders(row);
                        break;

                    default:
                        ProcessLines(row);
                        break;
                }
            }

            return new List<(int RowNum, IDictionary<string, object> Data)>(_lines);
        }

        private List<(int RowNum, IDictionary<string, object> Data)> ProcessCollectionSheet(XSSFWorkbook workbook, string entityName)
        {
            foreach (var activeSheet in new List<(string Sheet, string Entity, string DataSource)>(_sheets.Where(s => s.Entity.Equals(entityName) || s.Entity.StartsWith($"{entityName}."))))
            {
                var activeWorksheet = workbook.GetSheetAt(workbook.GetSheetIndex(activeSheet.Sheet));

                _dictionaryCollections.Add(activeSheet.Entity, ScrollRowsInCollectionSheet(activeWorksheet));

                ResetHeaders();

                _sheetsProcessed.Add(activeSheet.Sheet);
            }

            ProcessNestedCollections(_dictionaryCollections.Keys.Last());

            return new List<(int RowNum, IDictionary<string, object> Values)>(PrepareCollection());
        }

        private List<NestedCollection> ScrollRowsInCollectionSheet(ISheet activeWorksheet)
        {
            var importCollection = new List<NestedCollection>();

            for (var rowNum = 0; rowNum < activeWorksheet.PhysicalNumberOfRows; rowNum++)
            {
                var row = activeWorksheet.GetRow(rowNum);

                if (row == null) continue;

                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                switch (rowNum)
                {
                    case 0:
                        ProcessHeaders(row);
                        break;

                    default:
                        importCollection.Add(GetLinesCollection(row));
                        break;
                }
            }

            return importCollection;
        }

        private IEnumerable<(int RowNum, IDictionary<string, object>)> PrepareCollection()
        {
            var collection = new List<(int RowNum, IDictionary<string, object> Values)>();

            foreach (var item in _dictionaryCollections.FirstOrDefault().Value)
            {
                collection.Add(item.Data);
            }

            return collection;
        }

        private void LoadSheetNames(XSSFWorkbook workbook)
        {
            var configurationSheet = workbook.GetSheetAt(0);

            for (var rowNumber = 1; rowNumber < configurationSheet.PhysicalNumberOfRows; rowNumber++)
            {
                _sheets.Add((Sheet: configurationSheet.GetRow(rowNumber).GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString(),
                             Entity: configurationSheet.GetRow(rowNumber).GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString(),
                             DataSource: configurationSheet.GetRow(rowNumber).GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString()));
            }
        }

        private void ProcessNestedCollections(string childSheet)
        {
            var level = childSheet.Count(c => c.Equals('.'));
            if (level == 0) return;

            var importData = _dictionaryCollections[childSheet];

            var importCollection = _dictionaryCollections.FirstOrDefault(s => s.Key.Equals(GetParentSheet())).Value;

            foreach (var parent in importCollection)
            {
                var childData = importData.Where(i => i.ParentId == parent.Id);
                var field = childSheet.Split(".")[level];
                parent.Data.Values.Add(field, childData.Select(s => s.Data.Values));
            }

            _dictionaryCollections.Remove(childSheet);

            ProcessNestedCollections(_dictionaryCollections.Keys.Last());

            string GetParentSheet()
                => childSheet.Substring(0, childSheet.LastIndexOf('.'));
        }

        private NestedCollection GetLinesCollection(IRow row)
        {
            var collection = new NestedCollection
            {
                Data = (row.RowNum, Values: new Dictionary<string, object>())
            };

            for (var cellNum = 0; cellNum < row.LastCellNum; cellNum++)
            {
                LoadDataToCollection(row, collection, cellNum);
            }

            return collection;
        }

        private void LoadDataToCollection(IRow row, NestedCollection collection, int cellNum)
        {
            var cell = row.GetCell(cellNum, MissingCellPolicy.CREATE_NULL_AS_BLANK);

            var header = GetHeader();

            if (header.Equals(Id))
            {
                collection.Id = cell.ToString();
                return;
            }

            if (header.Equals(ParentId))
            {
                collection.ParentId = cell.ToString();
                return;
            }

            if (string.IsNullOrEmpty(header))
                return;

            switch (cell.CellType)
            {
                case CellType.String:
                    collection.Data.Values.Add(header, cell.StringCellValue);

                    break;

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        collection.Data.Values.Add(header, cell.DateCellValue);
                    }
                    else if (cell.CellStyle.DataFormat >= 164 && DateUtil.IsValidExcelDate(cell.NumericCellValue))
                    {
                        collection.Data.Values.Add(header, cell.DateCellValue);
                    }
                    else
                    {
                        collection.Data.Values.Add(header, cell.NumericCellValue);
                    }

                    break;

                case CellType.Boolean:
                    collection.Data.Values.Add(header, cell.BooleanCellValue);
                    break;

                case CellType.Formula:
                    collection.Data.Values.Add(header, cell.CellFormula);
                    break;

                case CellType.Error:
                    collection.Data.Values.Add(header, FormulaError.ForInt(cell.ErrorCellValue).String);
                    break;

                case CellType.Blank:
                    collection.Data.Values.Add(header, string.Empty);
                    break;

                case CellType.Unknown:
                    break;

                default:
                    throw new InvalidDataException($"Unknown Cell Type: {cell.CellType}");
            }

            string GetHeader() => _headers.Count > cellNum ? _headers[cellNum] : string.Empty;
        }

        private void LoadDataToLine(IRow row, IDictionary<string, object> line, int cellNum)
        {
            var cell = row.GetCell(cellNum, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            var header = GetHeader();

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

            string GetHeader() => _headers.Count > cellNum ? _headers[cellNum] : string.Empty;
        }

        private void ProcessLines(IRow row)
        {
            if (row == null) return;
            var line = new Dictionary<string, object>();
            for (var cellNum = 0; cellNum < row.LastCellNum; cellNum++)
            {
                LoadDataToLine(row, line, cellNum);
            }
            _lines.Add((row.RowNum, line));
        }

        private void ProcessHeaders(IRow row) => _headers.AddRange(row.Select(cell => cell.StringCellValue));

        private void ResetLines() => _lines.Clear();

        private void ResetHeaders() => _headers.Clear();

        private void ResetCollections() => _dictionaryCollections.Clear();

        private void ResetAllData()
        {
            ResetHeaders();
            ResetLines();
            ResetCollections();
        }
    }
}
