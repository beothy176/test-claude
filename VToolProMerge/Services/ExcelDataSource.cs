using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace VToolProMerge.Services;

/// <summary>
/// Nguồn dữ liệu Excel (.xlsx). Mỗi sheet trở thành 1 DataTable (key = tên sheet).
/// Sheet đặc biệt tên "Scalars" (nếu có) chứa cột "Key" và "Value" để map field
/// scalar — dùng cho các trường text/số/ngày dùng chung.
/// </summary>
public class ExcelDataSource : IDataSource
{
    private readonly Dictionary<string, object?> _scalars = new();
    private readonly Dictionary<string, DataTable> _tables = new();

    public string Name { get; }

    public ExcelDataSource(string filePath)
    {
        Name = Path.GetFileNameWithoutExtension(filePath);
        if (!File.Exists(filePath)) return;

        using var wb = new XLWorkbook(filePath);
        foreach (var ws in wb.Worksheets)
        {
            if (string.Equals(ws.Name, "Scalars", System.StringComparison.OrdinalIgnoreCase))
                LoadScalarsSheet(ws);
            else
                _tables[ws.Name] = LoadSheet(ws);
        }
    }

    public IReadOnlyDictionary<string, object?> GetScalars() => _scalars;
    public IReadOnlyDictionary<string, DataTable> GetTables() => _tables;

    private void LoadScalarsSheet(IXLWorksheet ws)
    {
        var rows = ws.RowsUsed().Skip(1); // bỏ header
        foreach (var row in rows)
        {
            var key = row.Cell(1).GetString();
            if (string.IsNullOrWhiteSpace(key)) continue;
            var c = row.Cell(2);
            if (c.DataType == XLDataType.Number) _scalars[key] = c.GetDouble();
            else if (c.DataType == XLDataType.DateTime) _scalars[key] = c.GetDateTime();
            else if (c.DataType == XLDataType.Boolean) _scalars[key] = c.GetBoolean();
            else _scalars[key] = c.GetString();
        }
    }

    private static DataTable LoadSheet(IXLWorksheet ws)
    {
        var dt = new DataTable(ws.Name);
        var first = ws.FirstRowUsed();
        if (first == null) return dt;

        var headers = first.CellsUsed().Select(c => c.GetString()).ToList();
        foreach (var h in headers) dt.Columns.Add(string.IsNullOrWhiteSpace(h) ? $"C{dt.Columns.Count + 1}" : h);

        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var dr = dt.NewRow();
            for (int i = 0; i < headers.Count; i++)
            {
                var c = row.Cell(i + 1);
                if (c.DataType == XLDataType.Number) dr[i] = c.GetDouble();
                else if (c.DataType == XLDataType.DateTime) dr[i] = c.GetDateTime();
                else dr[i] = c.GetString();
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }
}
