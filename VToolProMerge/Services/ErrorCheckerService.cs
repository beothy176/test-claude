using System.Collections.Generic;
using System.IO;
using System.Linq;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

public class ErrorCheckerService
{
    public IEnumerable<string> CheckMissingPlaceholders(
        IEnumerable<string> placeholdersInTemplate,
        IEnumerable<MappingRule> rules)
    {
        var mapped = rules.Select(r => r.Placeholder.Trim('[', ']')).ToHashSet();
        return placeholdersInTemplate.Where(p => !mapped.Contains(p));
    }

    public IEnumerable<string> CheckUnusedData(
        IEnumerable<string> placeholdersInTemplate,
        IEnumerable<MappingRule> rules)
    {
        var inTemplate = placeholdersInTemplate.ToHashSet();
        return rules.Select(r => r.Placeholder.Trim('[', ']'))
                    .Where(p => !inTemplate.Contains(p));
    }

    public IEnumerable<string> CheckMissingExcelSheet(string excelPath, IEnumerable<string> sheets)
    {
        var missing = new List<string>();
        if (!File.Exists(excelPath))
        {
            foreach (var s in sheets) missing.Add(s);
            return missing;
        }
        // TODO: dùng ClosedXML mở workbook và so sánh tên sheet.
        return missing;
    }

    public IEnumerable<string> CheckDynamicTableColumns(IEnumerable<string> required, IEnumerable<string> have)
    {
        var actual = have.ToHashSet();
        return required.Where(c => !actual.Contains(c));
    }

    public bool CheckTemplateFileExists(string path)
        => !string.IsNullOrEmpty(path) && File.Exists(path);
}
