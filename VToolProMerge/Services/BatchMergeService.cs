using System;
using System.Collections.Generic;
using System.IO;
using VToolProMerge.Helpers;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

public class BatchMergeService
{
    private readonly WordMergeEngine _word;
    private readonly AuditLogService _audit;

    public BatchMergeService(WordMergeEngine word, AuditLogService audit)
    {
        _word = word;
        _audit = audit;
    }

    public IReadOnlyList<string> ValidateBatch(IEnumerable<TemplateItem> templates)
    {
        var errors = new List<string>();
        foreach (var t in templates)
        {
            if (!File.Exists(t.FilePath))
                errors.Add($"Thiếu file mẫu: {t.Name}");
        }
        return errors;
    }

    public IReadOnlyList<string> GenerateDocuments(
        IEnumerable<TemplateItem> templates,
        IReadOnlyDictionary<string, string?> values,
        string outputDir,
        string fileNamePattern,
        string format)
    {
        Directory.CreateDirectory(outputDir);
        var outputs = new List<string>();
        foreach (var t in templates)
        {
            var nameValues = new Dictionary<string, string?>(values, StringComparer.Ordinal)
            {
                ["TEN_MAU"] = t.Name
            };
            var fileName = FileNameHelper.Resolve(fileNamePattern, nameValues) + ".docx";
            var fullPath = Path.Combine(outputDir, fileName);
            _word.MergeSingleDocument(t.FilePath, fullPath, values);
            outputs.Add(fullPath);

            if (format == "PDF" || format == "DOCX+PDF")
            {
                var pdf = Path.ChangeExtension(fullPath, ".pdf");
                try { _word.ExportToPdf(fullPath, pdf); outputs.Add(pdf); }
                catch (NotImplementedException) { /* sẽ nối sau */ }
            }

            _audit.WriteLog(new AuditLogItem
            {
                Action = $"Sinh hồ sơ: {t.Name}",
                Status = "Hoàn tất",
                Profile = outputDir
            });
        }
        return outputs;
    }

    public IReadOnlyList<string> ExportDocuments(IEnumerable<string> docxPaths, string format)
    {
        var result = new List<string>();
        foreach (var p in docxPaths)
        {
            result.Add(p);
            if (format == "PDF" || format == "DOCX+PDF")
            {
                var pdf = Path.ChangeExtension(p, ".pdf");
                try { _word.ExportToPdf(p, pdf); result.Add(pdf); }
                catch (NotImplementedException) { }
            }
        }
        return result;
    }

    public string BuildOutputFileName(string pattern, IReadOnlyDictionary<string, string?> values)
        => FileNameHelper.Resolve(pattern, values);
}
