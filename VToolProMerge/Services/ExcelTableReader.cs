using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace VToolProMerge.Services;

public class ExcelTableReader
{
    public DataTable ReadSheet(string filePath, string sheetName)
    {
        if (!File.Exists(filePath)) return new DataTable();
        using var wb = new XLWorkbook(filePath);
        if (!wb.TryGetWorksheet(sheetName, out var ws)) return new DataTable();
        return ToDataTable(ws);
    }

    public DataTable ReadTable(string filePath, string sheetName, string tableName)
    {
        if (!File.Exists(filePath)) return new DataTable();
        using var wb = new XLWorkbook(filePath);
        if (!wb.TryGetWorksheet(sheetName, out var ws)) return new DataTable();
        var tbl = ws.Tables.FirstOrDefault(t => t.Name == tableName);
        return tbl != null ? tbl.AsNativeDataTable() : ToDataTable(ws);
    }

    public IEnumerable<string> ValidateRequiredColumns(DataTable table, IEnumerable<string> required)
    {
        var have = new HashSet<string>();
        foreach (DataColumn c in table.Columns) have.Add(c.ColumnName);
        return required.Where(r => !have.Contains(r));
    }

    private static DataTable ToDataTable(IXLWorksheet ws)
    {
        var dt = new DataTable();
        var firstRow = ws.FirstRowUsed();
        if (firstRow == null) return dt;
        foreach (var cell in firstRow.CellsUsed())
            dt.Columns.Add(cell.GetString());

        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var dr = dt.NewRow();
            int i = 0;
            foreach (var cell in row.Cells(1, dt.Columns.Count))
            {
                dr[i++] = cell.GetString();
                if (i >= dt.Columns.Count) break;
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }
}
