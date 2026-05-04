using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace VToolProMerge.Services;

/// <summary>
/// Engine trộn Word. Bản khung dùng OpenXML để thay thế placeholder dạng [TEN_TRUONG].
/// Khi cần xuất PDF / preview thật, có thể chuyển sang Microsoft Office Interop.
/// </summary>
public class WordMergeEngine
{
    public void MergeSingleDocument(string sourceTemplate, string outputPath,
        IReadOnlyDictionary<string, string?> values)
    {
        File.Copy(sourceTemplate, outputPath, overwrite: true);
        ReplacePlaceholders(outputPath, values);
    }

    public void ReplacePlaceholders(string filePath, IReadOnlyDictionary<string, string?> values)
    {
        // TODO: Word thường tách 1 placeholder thành nhiều Run (split runs).
        // Bản production cần gộp run theo paragraph trước khi replace.
        using var doc = WordprocessingDocument.Open(filePath, true);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return;

        foreach (var t in body.Descendants<Text>())
        {
            foreach (var kv in values)
            {
                if (string.IsNullOrEmpty(t.Text)) continue;
                if (t.Text.Contains(kv.Key))
                    t.Text = t.Text.Replace(kv.Key, kv.Value ?? string.Empty);
            }
        }
        doc.MainDocumentPart!.Document.Save();
    }

    /// <summary>
    /// Xuất PDF. Bản hiện tại chỉ là khung — sẽ nối với Microsoft Word Interop:
    ///   wordApp.Documents.Open(...).ExportAsFixedFormat(pdfPath, WdExportFormat.wdExportFormatPDF);
    /// </summary>
    public void ExportToPdf(string docxPath, string pdfPath)
    {
        // TODO: Implement bằng Microsoft.Office.Interop.Word.
        throw new System.NotImplementedException(
            "Cần Microsoft Word cài trên máy + tham chiếu Office Interop để xuất PDF thật.");
    }
}
