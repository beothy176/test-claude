using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

public class ErrorCheckerService
{
    public IEnumerable<string> CheckMissingPlaceholders(
        IEnumerable<string> placeholdersInTemplate, MergeContext ctx)
    {
        return placeholdersInTemplate.Where(p =>
        {
            var key = $"[{p}]";
            return !ctx.Tokens.TryGetValue(key, out var v) || string.IsNullOrEmpty(v);
        });
    }

    public IEnumerable<string> CheckUnusedData(
        IEnumerable<string> placeholdersInTemplate, MergeContext ctx)
    {
        var inTemplate = placeholdersInTemplate.ToHashSet();
        return ctx.Tokens.Keys
            .Select(k => k.Trim('[', ']'))
            .Where(p => !inTemplate.Contains(p));
    }

    public IEnumerable<string> CheckMissingExcelSheet(string excelPath, IEnumerable<string> sheets)
    {
        if (!File.Exists(excelPath)) return sheets;
        using var wb = new XLWorkbook(excelPath);
        var actual = wb.Worksheets.Select(w => w.Name).ToHashSet();
        return sheets.Where(s => !actual.Contains(s));
    }

    public IEnumerable<string> CheckDynamicTableColumns(IEnumerable<string> required, IEnumerable<string> have)
    {
        var actual = have.ToHashSet();
        return required.Where(c => !actual.Contains(c));
    }

    public bool CheckTemplateFileExists(string path)
        => !string.IsNullOrEmpty(path) && File.Exists(path);
}
