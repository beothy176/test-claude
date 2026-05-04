using System.Collections.Generic;

namespace VToolProMerge.Services;

public class PreviewService
{
    private readonly List<string> _logs = new();

    public string CreatePreviewDocument(string templatePath,
        IReadOnlyDictionary<string, string?> sampleData)
    {
        // TODO: copy template ra %TEMP%, chạy WordMergeEngine, trả đường dẫn file preview.
        return string.Empty;
    }

    public void OpenInWord(string filePath)
    {
        // TODO: nối Microsoft Word Interop để mở file preview trong Word.
        // var word = new Microsoft.Office.Interop.Word.Application { Visible = true };
        // word.Documents.Open(filePath);
    }

    public IReadOnlyList<string> LogPreviewErrors() => _logs;
    public void AddLog(string msg) => _logs.Add(msg);
}
