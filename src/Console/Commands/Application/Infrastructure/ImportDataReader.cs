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
        private const string ID = "#ID";
        private const string ParentID = "ParentID";
        private readonly List<string> _headers = new List<string>();
        private readonly List<IDictionary<string, object>> _lines = new List<IDictionary<string, object>>();
        private readonly List<ImportData> _data = new List<ImportData>();
        private readonly List<string> _sheets = new List<string>();
        private readonly List<string> _sheetsProcessed = new List<string>();

        private readonly Dictionary<string, List<NestedCollection>> _dictionaryCollections = new Dictionary<string, List<NestedCollection>>();

        public IList<ImportData> ReadExcel(string path)
        {
            var workbook = new XSSFWorkbook(path);

            LoadSheetNames(workbook);

            ScrollSheets(workbook);

            return _data;
        }

        private void ScrollSheets(XSSFWorkbook workbook)
        {
            foreach (var sheet in _sheets)
            {
                if (_sheetsProcessed.Contains(sheet))
                {
                    continue;
                }

                var activeWorksheet = workbook.GetSheetAt(workbook.GetSheetIndex(sheet));

                var namingParts = GetSheetNameWithoutNamingKey(activeWorksheet.SheetName).Split('.');
                var entityName = namingParts[0];
                var dataSource = GetDataSource(namingParts);

                var lines = new List<IDictionary<string, object>>();

                if (_sheets.Any(s => s.StartsWith($"{sheet}.")))
                {
                    lines = ProcessCollectionSheet(workbook, sheet);
                }
                else
                {
                    lines = ProcessSimpleSheet(activeWorksheet);
                }

                _data.Add(new ImportData(entityName, dataSource, lines));

                ResetAllData();
            }

            string GetSheetNameWithoutNamingKey(string sheetName)
              => sheetName.Split('-')[0];

            string GetDataSource(string[] sheetNameParts)
              => sheetNameParts.Length > 1 ? sheetNameParts[1] : "default";
        }

        private List<IDictionary<string, object>> ProcessSimpleSheet(ISheet activeWorksheet)
        {
            if (activeWorksheet.PhysicalNumberOfRows == 0)
                throw new Exception($"The sheet {activeWorksheet.SheetName} is Empty");

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

            return new List<IDictionary<string, object>>(_lines);
        }

        private List<IDictionary<string, object>> ProcessCollectionSheet(XSSFWorkbook workbook, string sheet)
        {
            foreach (var activeSheet in new List<string>(_sheets.Where(s => s.StartsWith(sheet))))
            {
                var activeWorksheet = workbook.GetSheetAt(workbook.GetSheetIndex(activeSheet));

                _dictionaryCollections.Add(activeSheet, ScrollRowsInCollectionSheet(activeWorksheet));

                ResetHeaders();

                _sheetsProcessed.Add(activeSheet);
            }

            ProcessNestedCollections(_dictionaryCollections.Keys.Last());

            return new List<IDictionary<string, object>>(PrepareCollection());
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

        private IEnumerable<IDictionary<string, object>> PrepareCollection()
        {
            var collection = new List<IDictionary<string, object>>();

            foreach (var item in _dictionaryCollections.FirstOrDefault().Value)
            {
                collection.Add(item.Data);
            }

            return collection;
        }

        private void LoadSheetNames(XSSFWorkbook workbook)
        {
            for (var sheetNumber = 0; sheetNumber < workbook.NumberOfSheets; sheetNumber++)
            {
                _sheets.Add(workbook.GetSheetAt(sheetNumber).SheetName);
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
                parent.Data.Add(field, childData.Select(s => s.Data));
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
                Data = new Dictionary<string, object>()
            };

            if (row == null) return null;

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

            if (header.Equals(ID))
            {
                collection.Id = cell.ToString();
                return;
            }
            else if (header.Equals(ParentID))
            {
                collection.ParentId = cell.ToString();
                return;
            }
            else if (string.IsNullOrEmpty(header))
                return;

            switch (cell.CellType)
            {
                case CellType.String:
                    collection.Data.Add(header, cell.StringCellValue);
                    break;

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        collection.Data.Add(header, cell.DateCellValue);
                    }
                    else if (cell.CellStyle.DataFormat >= 164 && DateUtil.IsValidExcelDate(cell.NumericCellValue))
                    {
                        collection.Data.Add(header, cell.DateCellValue);
                    }
                    else
                    {
                        collection.Data.Add(header, cell.NumericCellValue);
                    }

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
            _lines.Add(line);
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
