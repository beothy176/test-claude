using System.Collections.Generic;
using System.Data;

namespace VToolProMerge.Services;

/// <summary>
/// Nguồn dữ liệu trộn. Mỗi nguồn cung cấp:
///   - Map field -> giá trị scalar (text/số/ngày).
///   - Bảng dữ liệu lặp dòng (key = prefix, vd "HH" cho [HH_*]).
/// </summary>
public interface IDataSource
{
    string Name { get; }
    IReadOnlyDictionary<string, object?> GetScalars();
    IReadOnlyDictionary<string, DataTable> GetTables();
}
