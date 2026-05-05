using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;

namespace VToolProMerge.Services;

/// <summary>
/// Nguồn dữ liệu JSON. Cấu trúc kỳ vọng:
/// {
///   "Scalars": { "DONVI_TEN": "Cty ABC", "SO_HOP_DONG": "001/2025", ... },
///   "Tables": {
///     "HH": [
///       { "STT": 1, "MA": "M01", "TEN": "...", "DVT": "Cái", "SL": 10, "DG": 20000, "TT": 200000 }
///     ]
///   }
/// }
/// </summary>
public class JsonDataSource : IDataSource
{
    private readonly Dictionary<string, object?> _scalars = new();
    private readonly Dictionary<string, DataTable> _tables = new();

    public string Name { get; }

    public JsonDataSource(string filePath)
    {
        Name = Path.GetFileNameWithoutExtension(filePath);
        if (!File.Exists(filePath)) return;

        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("Scalars", out var scalars) && scalars.ValueKind == JsonValueKind.Object)
        {
            foreach (var kv in scalars.EnumerateObject())
                _scalars[kv.Name] = ReadValue(kv.Value);
        }

        if (root.TryGetProperty("Tables", out var tables) && tables.ValueKind == JsonValueKind.Object)
        {
            foreach (var kv in tables.EnumerateObject())
                _tables[kv.Name] = JsonArrayToDataTable(kv.Value, kv.Name);
        }
    }

    public IReadOnlyDictionary<string, object?> GetScalars() => _scalars;
    public IReadOnlyDictionary<string, DataTable> GetTables() => _tables;

    private static object? ReadValue(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.TryGetDecimal(out var d) ? (object)d : e.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => e.ToString()
    };

    private static DataTable JsonArrayToDataTable(JsonElement arr, string name)
    {
        var dt = new DataTable(name);
        if (arr.ValueKind != JsonValueKind.Array) return dt;
        foreach (var row in arr.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Object) continue;
            foreach (var prop in row.EnumerateObject())
                if (!dt.Columns.Contains(prop.Name)) dt.Columns.Add(prop.Name);
            var dr = dt.NewRow();
            foreach (var prop in row.EnumerateObject())
                dr[prop.Name] = ReadValue(prop.Value) ?? System.DBNull.Value;
            dt.Rows.Add(dr);
        }
        return dt;
    }
}
