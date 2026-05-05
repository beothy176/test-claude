using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using VToolProMerge.Helpers;

namespace VToolProMerge.Services;

/// <summary>
/// Engine trộn Word thật. Thay placeholder dạng [TEN_TRUONG] an toàn cho trường hợp
/// Word tách 1 chuỗi thành nhiều Run (split-runs). Áp dụng cho body, header, footer,
/// footnote, endnote. Xuất PDF qua Microsoft Word COM (Reflection late-bound, không
/// bắt buộc cài Office trên máy build).
/// </summary>
public class WordMergeEngine
{
    public void MergeSingleDocument(string sourceTemplate, string outputPath,
        IReadOnlyDictionary<string, string?> values,
        ConditionalBlockProcessor? conditional = null,
        DynamicTableMerger? tableMerger = null,
        IReadOnlyDictionary<string, System.Data.DataTable>? tableSources = null)
    {
        if (!File.Exists(sourceTemplate))
            throw new FileNotFoundException("Không tìm thấy file mẫu", sourceTemplate);

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.Copy(sourceTemplate, outputPath, overwrite: true);

        // Pipeline: bảng động -> điều kiện -> placeholder.
        if (tableMerger != null && tableSources != null && tableSources.Count > 0)
            tableMerger.MergeTables(outputPath, tableSources);

        if (conditional != null)
            conditional.ApplyConditionalBlocks(outputPath, values);

        ReplacePlaceholders(outputPath, values);
    }

    public void ReplacePlaceholders(string filePath, IReadOnlyDictionary<string, string?> values)
    {
        using var doc = WordprocessingDocument.Open(filePath, true);

        foreach (var part in OpenXmlHelpers.AllStoryParts(doc))
        {
            var root = OpenXmlHelpers.GetRoot(part);
            if (root == null) continue;
            OpenXmlHelpers.ReplacePlaceholdersInElement(root, values);
        }

        doc.MainDocumentPart!.Document.Save();
    }

    /// <summary>
    /// Xuất DOCX sang PDF qua Microsoft Word COM (Reflection late-bound — không cần
    /// reference Microsoft.Office.Interop.Word lúc build). Yêu cầu Word cài trên máy
    /// chạy.
    /// </summary>
    public void ExportToPdf(string docxPath, string pdfPath)
    {
        var wordType = Type.GetTypeFromProgID("Word.Application")
            ?? throw new InvalidOperationException(
                "Microsoft Word chưa cài trên máy này — không thể xuất PDF.");

        var word = Activator.CreateInstance(wordType)!;
        try
        {
            Set(word, wordType, "Visible", false);
            Set(word, wordType, "DisplayAlerts", 0);

            var documents = Get(word, wordType, "Documents")!;
            var doc = Invoke(documents, "Open", new object[]
            {
                Path.GetFullPath(docxPath),
                false, // ConfirmConversions
                true   // ReadOnly
            })!;
            try
            {
                // wdExportFormatPDF = 17
                Invoke(doc, "ExportAsFixedFormat", new object[]
                {
                    Path.GetFullPath(pdfPath),
                    17 // ExportFormat
                });
            }
            finally
            {
                Invoke(doc, "Close", new object[] { 0 }); // wdDoNotSaveChanges
            }
        }
        finally
        {
            Invoke(word, "Quit", new object[] { 0 });
        }
    }

    private static void Set(object target, Type type, string name, object value)
        => type.InvokeMember(name, BindingFlags.SetProperty, null, target, new[] { value });

    private static object? Get(object target, Type type, string name)
        => type.InvokeMember(name, BindingFlags.GetProperty, null, target, null);

    private static object? Invoke(object target, string name, object[] args)
        => target.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, target, args);
}
