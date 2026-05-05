using System.Collections.Generic;
using System.Data;

namespace VToolProMerge.Services;

/// <summary>
/// Context cho 1 lần trộn 1 file: các giá trị scalar + các bảng dữ liệu (key = prefix).
/// </summary>
public class MergeContext
{
    public Dictionary<string, string?> Tokens { get; } = new(); // key = "[FIELD]"
    public Dictionary<string, DataTable> TableSources { get; } = new(); // key = prefix vd "HH"
}
