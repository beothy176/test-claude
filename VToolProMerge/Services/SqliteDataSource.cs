using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace VToolProMerge.Services;

/// <summary>
/// Nguồn dữ liệu SQLite. Đọc:
///   - SELECT Key, Value FROM Scalars  -> field scalar
///   - SELECT * FROM &lt;table&gt;          -> bảng dữ liệu (mỗi bảng có whitelist)
/// Truyền tableNames = whitelist danh sách bảng cần lấy.
/// </summary>
public class SqliteDataSource : IDataSource
{
    private readonly Dictionary<string, object?> _scalars = new();
    private readonly Dictionary<string, DataTable> _tables = new();

    public string Name { get; }

    public SqliteDataSource(string dbPath, IEnumerable<string> tableNames)
    {
        Name = Path.GetFileNameWithoutExtension(dbPath);
        if (!File.Exists(dbPath)) return;

        var cs = $"Data Source={dbPath}";
        using var con = new SqliteConnection(cs);
        con.Open();

        TryLoadScalars(con);
        foreach (var t in tableNames) TryLoadTable(con, t);
    }

    public IReadOnlyDictionary<string, object?> GetScalars() => _scalars;
    public IReadOnlyDictionary<string, DataTable> GetTables() => _tables;

    private void TryLoadScalars(SqliteConnection con)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Key, Value FROM Scalars";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) _scalars[rdr.GetString(0)] = rdr.IsDBNull(1) ? null : rdr.GetValue(1);
        }
        catch { /* không có bảng Scalars — bỏ qua */ }
    }

    private void TryLoadTable(SqliteConnection con, string tableName)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT * FROM \"{tableName}\"";
            using var rdr = cmd.ExecuteReader();
            var dt = new DataTable(tableName);
            for (int i = 0; i < rdr.FieldCount; i++) dt.Columns.Add(rdr.GetName(i));
            while (rdr.Read())
            {
                var dr = dt.NewRow();
                for (int i = 0; i < rdr.FieldCount; i++)
                    dr[i] = rdr.IsDBNull(i) ? System.DBNull.Value : rdr.GetValue(i);
                dt.Rows.Add(dr);
            }
            _tables[tableName] = dt;
        }
        catch { /* bảng không tồn tại — bỏ qua */ }
    }
}
