using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using VToolProMerge.Helpers;

namespace VToolProMerge.Services;

/// <summary>
/// Trộn bảng động vào Word. Quy ước:
///   - Mỗi bảng dữ liệu được nhận diện qua "prefix" của placeholder (vd: HH_).
///   - Trong Word, dòng template là dòng có ít nhất 2 ô chứa placeholder dạng [PREFIX_*].
///   - Các DataTable nguồn dùng tên cột bằng phần sau prefix (vd: STT, MA, TEN, ...).
///   - Dòng tổng (chứa [TONG_*]) sẽ được giữ nguyên và xử lý ở WordMergeEngine
///     (do nó là placeholder thường, không phải bảng động).
/// </summary>
public class DynamicTableMerger
{
    private static readonly Regex CellTokenRegex = new(@"\[([A-Z][A-Z0-9_]+)\]",
        RegexOptions.Compiled);

    /// <summary>
    /// <paramref name="tableSources"/>: key = prefix (vd "HH"), value = dữ liệu lặp dòng.
    /// </summary>
    public void MergeTables(string docxPath, IReadOnlyDictionary<string, DataTable> tableSources)
    {
        using var doc = WordprocessingDocument.Open(docxPath, true);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return;

        foreach (var table in body.Descendants<Table>().ToList())
        {
            foreach (var (prefix, dt) in tableSources)
            {
                MergeOneTable(table, prefix, dt);
            }
        }

        doc.MainDocumentPart!.Document.Save();
    }

    private void MergeOneTable(Table table, string prefix, DataTable rows)
    {
        var allRows = table.Elements<TableRow>().ToList();
        var templateRow = FindTemplateRow(allRows, prefix);
        if (templateRow == null) return;

        // Clone -> insert before template -> remove template.
        var parent = templateRow.Parent ?? table;
        foreach (DataRow dr in rows.Rows)
        {
            var clone = (TableRow)templateRow.CloneNode(true);
            ReplaceTokensInRow(clone, prefix, dr);
            parent.InsertBefore(clone, templateRow);
        }
        templateRow.Remove();
    }

    private static TableRow? FindTemplateRow(List<TableRow> rows, string prefix)
    {
        // "Prefix" có thể chứa _, vd HH. Token mong muốn: [HH_*]
        var pattern = new Regex(@"\[" + Regex.Escape(prefix) + @"_[A-Z0-9_]+\]",
            RegexOptions.Compiled);

        foreach (var r in rows)
        {
            int hits = 0;
            foreach (var c in r.Elements<TableCell>())
            {
                if (pattern.IsMatch(string.Concat(c.Descendants<Text>().Select(t => t.Text))))
                    hits++;
            }
            if (hits >= 1) return r;
        }
        return null;
    }

    private static void ReplaceTokensInRow(TableRow row, string prefix, DataRow data)
    {
        var prefixLen = prefix.Length + 2; // "[HH_"
        foreach (var p in row.Descendants<Paragraph>())
        {
            var texts = p.Descendants<Text>().ToList();
            if (texts.Count == 0) continue;
            var full = string.Concat(texts.Select(t => t.Text));
            if (string.IsNullOrEmpty(full)) continue;
            if (!full.Contains('[')) continue;

            var replaced = CellTokenRegex.Replace(full, m =>
            {
                var key = m.Groups[1].Value; // ví dụ HH_TEN
                if (!key.StartsWith(prefix + "_", StringComparison.Ordinal))
                    return m.Value;
                var col = key.Substring(prefix.Length + 1); // "TEN"
                if (!data.Table.Columns.Contains(col)) return string.Empty;
                var v = data[col];
                return FormatValue(v);
            });

            if (replaced == full) continue;
            texts[0].Text = replaced;
            texts[0].Space = SpaceProcessingModeValues.Preserve;
            for (int i = 1; i < texts.Count; i++) texts[i].Text = string.Empty;
        }
    }

    private static string FormatValue(object? v)
    {
        if (v == null || v == DBNull.Value) return string.Empty;
        return v switch
        {
            decimal d => d.ToString("#,##0.##", CultureInfo.GetCultureInfo("vi-VN")),
            double db => db.ToString("#,##0.##", CultureInfo.GetCultureInfo("vi-VN")),
            float f => f.ToString("#,##0.##", CultureInfo.GetCultureInfo("vi-VN")),
            int i => i.ToString(CultureInfo.GetCultureInfo("vi-VN")),
            DateTime dt => dt.ToString("dd/MM/yyyy"),
            _ => v.ToString() ?? string.Empty
        };
    }

    public decimal CalculateTotal(DataTable rows, string columnName)
    {
        decimal sum = 0;
        if (!rows.Columns.Contains(columnName)) return 0;
        foreach (DataRow r in rows.Rows)
        {
            if (decimal.TryParse(r[columnName]?.ToString(), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var v)) sum += v;
        }
        return sum;
    }

    public decimal ApplyVat(decimal amount, decimal vatPercent)
        => amount + amount * vatPercent / 100m;

    public string FormatNumber(decimal v, string format = "#,##0.##")
        => v.ToString(format, CultureInfo.GetCultureInfo("vi-VN"));
}
