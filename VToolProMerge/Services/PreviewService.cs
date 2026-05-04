using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VToolProMerge.Services;

/// <summary>
/// Tạo bản preview cho 1 mẫu: copy template ra %TEMP% -> trộn -> mở Word qua COM.
/// Dùng Reflection thuần thay vì dynamic để tránh phụ thuộc Microsoft.CSharp.
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
    /// Mở file trong Microsoft Word qua COM (Reflection late-bound).
    /// Nếu Word chưa cài, fallback sang shell mặc định.
    /// </summary>
    public void OpenInWord(string filePath)
    {
        var wordType = Type.GetTypeFromProgID("Word.Application");
        if (wordType == null)
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            AddLog("Mở bằng shell vì không tìm thấy Microsoft Word.");
            return;
        }
        var word = Activator.CreateInstance(wordType)!;
        wordType.InvokeMember("Visible",
            BindingFlags.SetProperty, null, word, new object[] { true });
        var documents = wordType.InvokeMember("Documents",
            BindingFlags.GetProperty, null, word, null)!;
        documents.GetType().InvokeMember("Open",
            BindingFlags.InvokeMethod, null, documents,
            new object[] { Path.GetFullPath(filePath) });
    }

    public IReadOnlyList<string> LogPreviewErrors() => _logs;
    public void AddLog(string msg) => _logs.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
}
