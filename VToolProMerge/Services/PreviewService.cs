using System;
using System.Collections.Generic;
using System.IO;

namespace VToolProMerge.Services;

/// <summary>
/// Tạo bản preview cho 1 mẫu: copy template ra %TEMP% -> trộn -> mở Word (Interop late-bound).
/// </summary>
public class PreviewService
{
    private readonly WordMergeEngine _word;
    private readonly List<string> _logs = new();

    public PreviewService(WordMergeEngine word)
    {
        _word = word;
    }

    public string CreatePreviewDocument(string templatePath, MergeContext ctx,
        ConditionalBlockProcessor? cond = null,
        DynamicTableMerger? tables = null)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Không tìm thấy file mẫu", templatePath);

        var preview = Path.Combine(
            Path.GetTempPath(),
            $"VToolPro_Preview_{DateTime.Now:yyyyMMdd_HHmmss}_" +
            Path.GetFileNameWithoutExtension(templatePath) + ".docx");

        _word.MergeSingleDocument(templatePath, preview, ctx.Tokens,
            cond, tables, ctx.TableSources);
        AddLog($"Preview generated: {preview}");
        return preview;
    }

    /// <summary>
    /// Mở file trong Microsoft Word. Yêu cầu Word đã cài đặt.
    /// Nếu không tìm thấy COM, fallback sang shell (Process.Start).
    /// </summary>
    public void OpenInWord(string filePath)
    {
        var wordType = Type.GetTypeFromProgID("Word.Application");
        if (wordType == null)
        {
            // Fallback: mở bằng app mặc định.
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            AddLog("Mở bằng shell vì không tìm thấy Microsoft Word.");
            return;
        }
        dynamic word = Activator.CreateInstance(wordType)!;
        word.Visible = true;
        word.Documents.Open(Path.GetFullPath(filePath));
    }

    public IReadOnlyList<string> LogPreviewErrors() => _logs;
    public void AddLog(string msg) => _logs.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
}
