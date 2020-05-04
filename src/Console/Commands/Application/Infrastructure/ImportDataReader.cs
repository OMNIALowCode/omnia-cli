using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Application.Infrastructure
{
    internal class ImportDataReader
    {
        private readonly List<string> _headers = new List<string>();
        private readonly List<IDictionary<string, object>> _lines = new List<IDictionary<string, object>>();
        private readonly List<ImportData> _data = new List<ImportData>();
        private readonly List<string> _sheets = new List<string>();
        private readonly List<string> _sheetsProcessed = new List<string>();

        private readonly Dictionary<string, List<ImportCollection>> _dictionaryCollections = new Dictionary<string, List<ImportCollection>>();
        public IList<ImportData> ReadExcel(string path)
        {
            var workbook = new XSSFWorkbook(path);

            LoadSheets(workbook);

            foreach (var sheet in _sheets)
            {
                if (_sheetsProcessed.Contains(sheet))
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

            return _data;

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

            _data.Add(new ImportData(entityName, dataSource, new List<IDictionary<string, object>>(_lines)));
            ResetData();
        }

        private void ProcessEntityWithCollections(XSSFWorkbook workbook, string sheet, string entityName, string dataSource)
        {
            foreach (var item in new List<string>(_sheets.Where(s => s.StartsWith(sheet))))
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
                _sheetsProcessed.Add(item);
            }

            ProcessHierarchy(_dictionaryCollections.Keys.Last());

            var collection = PrepareCollection();
            _data.Add(new ImportData(entityName, dataSource, new List<IDictionary<string, object>>(collection)));

            ResetData();
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

        private void ResetData()
        {
            _lines.Clear();
            _headers.Clear();
            _dictionaryCollections.Clear();
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

            var importData = _dictionaryCollections[entity];

            var importCollection = _dictionaryCollections.FirstOrDefault(s => s.Key.Equals(entity.Substring(0, entity.LastIndexOf('.')))).Value;
            foreach (var parent in importCollection)
            {
                var childData = importData.Where(i => i.ParentId == parent.Id);
                var field = entity.Split(".")[level];
                parent.Data.Add(field, childData.Select(s => s.Data));
            }
            _dictionaryCollections.Remove(entity);
            ProcessHierarchy(_dictionaryCollections.Keys.Last());
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
    }
}
