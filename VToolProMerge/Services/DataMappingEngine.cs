using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

/// <summary>
/// Engine nối nhiều IDataSource lại + áp dụng MappingRule -> ra MergeContext sẵn sàng
/// để WordMergeEngine/DynamicTableMerger sử dụng.
/// Quy ước key map:
///   - Scalars: key trong DataSource trùng FieldName của placeholder (vd "DONVI_TEN").
///   - Tables: key trong DataSource = prefix của placeholder (vd "HH" cho [HH_*]).
/// </summary>
public class DataMappingEngine
{
    public MergeContext BuildContext(IEnumerable<IDataSource> sources,
        IEnumerable<MappingRule>? rules = null)
    {
        var ctx = new MergeContext();

        // 1) Gộp scalars từ tất cả nguồn (nguồn sau ghi đè nguồn trước).
        var pool = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var src in sources)
            foreach (var kv in src.GetScalars())
                pool[kv.Key] = kv.Value;

        // 2) Áp dụng MappingRule (nếu có) — cho phép đổi tên/giá trị mặc định/required.
        if (rules != null)
        {
            foreach (var r in rules)
            {
                var fieldName = r.Placeholder.Trim('[', ']');
                if (!pool.TryGetValue(r.SourceField, out var v) || v == null
                    || string.IsNullOrWhiteSpace(v.ToString()))
                {
                    pool[fieldName] = r.DefaultValue;
                }
                else
                {
                    pool[fieldName] = v;
                }
            }
        }

        // 3) Build dictionary token -> string.
        foreach (var kv in pool)
            ctx.Tokens[$"[{kv.Key}]"] = FormatScalar(kv.Value);

        // 4) Gộp tables.
        foreach (var src in sources)
            foreach (var t in src.GetTables())
                ctx.TableSources[t.Key] = t.Value;

        return ctx;
    }

    public IEnumerable<string> GetMissingFields(IEnumerable<string> placeholdersInTemplate, MergeContext ctx)
        => placeholdersInTemplate.Where(p =>
        {
            var key = $"[{p}]";
            return !ctx.Tokens.TryGetValue(key, out var v) || string.IsNullOrEmpty(v);
        });

    public IEnumerable<MappingRule> ValidateRequiredFields(IEnumerable<MappingRule> rules, MergeContext ctx)
        => rules.Where(r => r.Required &&
            (!ctx.Tokens.TryGetValue(r.Placeholder, out var v) || string.IsNullOrEmpty(v)));

    private static string FormatScalar(object? v)
    {
        if (v == null) return string.Empty;
        var vi = CultureInfo.GetCultureInfo("vi-VN");
        return v switch
        {
            decimal d => d.ToString("#,##0.##", vi),
            double db => db.ToString("#,##0.##", vi),
            float f => f.ToString("#,##0.##", vi),
            int i => i.ToString(vi),
            long l => l.ToString(vi),
            DateTime dt => dt.ToString("dd/MM/yyyy"),
            bool b => b ? "true" : "false",
            _ => v.ToString() ?? string.Empty
        };
    }
}
