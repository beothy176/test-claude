using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VToolProMerge.Helpers;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

/// <summary>
/// Sinh hàng loạt văn bản trong 1 bộ hồ sơ. Gọi đầy đủ:
///   ConditionalBlockProcessor -> DynamicTableMerger -> WordMergeEngine.ReplacePlaceholders
///   -> ExportToPdf (nếu cần).
/// </summary>
public class BatchMergeService
{
    private readonly WordMergeEngine _word;
    private readonly DynamicTableMerger _tables;
    private readonly ConditionalBlockProcessor _conditional;
    private readonly AuditLogService _audit;

    public BatchMergeService(
        WordMergeEngine word,
        DynamicTableMerger tables,
        ConditionalBlockProcessor conditional,
        AuditLogService audit)
    {
        _word = word;
        _tables = tables;
        _conditional = conditional;
        _audit = audit;
    }

    public IReadOnlyList<string> ValidateBatch(IEnumerable<TemplateItem> templates)
    {
        var errors = new List<string>();
        foreach (var t in templates)
        {
            if (string.IsNullOrEmpty(t.FilePath) || !File.Exists(t.FilePath))
                errors.Add($"Thiếu file mẫu: {t.Name}");
        }
        return errors;
    }

    public class GenerateResult
    {
        public List<string> OutputFiles { get; } = new();
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();
        public int FilesDone { get; set; }
        public int FilesTotal { get; set; }
    }

    public GenerateResult GenerateDocuments(
        IEnumerable<TemplateItem> templates,
        MergeContext context,
        string outputDir,
        string fileNamePattern,
        string format,
        Action<int, int>? onProgress = null)
    {
        var result = new GenerateResult();
        var list = templates.ToList();
        result.FilesTotal = list.Count;
        Directory.CreateDirectory(outputDir);

        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];
            try
            {
                if (string.IsNullOrEmpty(t.FilePath) || !File.Exists(t.FilePath))
                {
                    result.Errors.Add($"Bỏ qua {t.Name}: file mẫu không tồn tại.");
                    continue;
                }

                var fileNameValues = new Dictionary<string, string?>(StringComparer.Ordinal);
                foreach (var kv in context.Tokens)
                    fileNameValues[kv.Key.Trim('[', ']')] = kv.Value;
                fileNameValues["TEN_MAU"] = t.Name;

                var docxName = FileNameHelper.Resolve(fileNamePattern, fileNameValues) + ".docx";
                var docxPath = Path.Combine(outputDir, docxName);

                _word.MergeSingleDocument(
                    sourceTemplate: t.FilePath,
                    outputPath: docxPath,
                    values: context.Tokens,
                    conditional: _conditional,
                    tableMerger: _tables,
                    tableSources: context.TableSources);

                if (format != "PDF") result.OutputFiles.Add(docxPath);

                if (format == "PDF" || format == "DOCX+PDF")
                {
                    var pdfPath = Path.ChangeExtension(docxPath, ".pdf");
                    try
                    {
                        _word.ExportToPdf(docxPath, pdfPath);
                        result.OutputFiles.Add(pdfPath);
                        if (format == "PDF" && File.Exists(docxPath)) File.Delete(docxPath);
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Không xuất được PDF cho {t.Name}: {ex.Message}");
                    }
                }

                _audit.WriteLog(new AuditLogItem
                {
                    Action = $"Sinh hồ sơ: {t.Name}",
                    Status = "Hoàn tất",
                    Profile = outputDir
                });

                result.FilesDone++;
                onProgress?.Invoke(result.FilesDone, result.FilesTotal);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Lỗi với {t.Name}: {ex.Message}");
                _audit.WriteLog(new AuditLogItem
                {
                    Action = $"Sinh hồ sơ: {t.Name}",
                    Status = "Lỗi",
                    Profile = outputDir,
                    Detail = ex.Message
                });
            }
        }

        return result;
    }

    public string BuildOutputFileName(string pattern, IReadOnlyDictionary<string, string?> values)
        => FileNameHelper.Resolve(pattern, values);
}
