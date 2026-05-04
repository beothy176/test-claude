using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using VToolProMerge.Helpers;

namespace VToolProMerge.Services;

/// <summary>
/// Engine trộn Word thật. Thay placeholder dạng [TEN_TRUONG] an toàn cho trường hợp
/// Word tách 1 chuỗi thành nhiều Run (split-runs). Áp dụng cho body, header, footer,
/// footnote, endnote. Xuất PDF qua Microsoft Word Interop (late-bound COM, không bắt
/// buộc cài Office trên máy build).
/// </summary>
public class WordMergeEngine
{
    /// <summary>
    /// Tạo file output bằng cách copy template, chạy điều kiện, thay placeholder.
    /// Bảng động được xử lý ở DynamicTableMerger (gọi sau khi ReplacePlaceholders).
    /// </summary>
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

        // Thứ tự xử lý:
        //  1) Bảng động (clone row trước khi thay placeholder để placeholder trong bảng
        //     được nhân bản đúng theo từng dòng dữ liệu).
        //  2) Khối điều kiện (xóa block khi điều kiện sai).
        //  3) Thay placeholder.
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
    /// Xuất DOCX sang PDF qua Microsoft Word Interop (COM trễ — không cần reference Interop).
    /// Yêu cầu Microsoft Word đã cài trên máy chạy.
    /// </summary>
    public void ExportToPdf(string docxPath, string pdfPath)
    {
        var wordType = Type.GetTypeFromProgID("Word.Application")
            ?? throw new InvalidOperationException(
                "Microsoft Word chưa cài trên máy này — không thể xuất PDF.");

        dynamic word = Activator.CreateInstance(wordType)!;
        try
        {
            word.Visible = false;
            word.DisplayAlerts = 0; // wdAlertsNone
            dynamic doc = word.Documents.Open(
                FileName: Path.GetFullPath(docxPath),
                ConfirmConversions: false,
                ReadOnly: true,
                AddToRecentFiles: false);
            try
            {
                // wdExportFormatPDF = 17
                doc.ExportAsFixedFormat(
                    OutputFileName: Path.GetFullPath(pdfPath),
                    ExportFormat: 17,
                    OpenAfterExport: false,
                    OptimizeFor: 0,           // wdExportOptimizeForPrint
                    Range: 0,                  // wdExportAllDocument
                    Item: 7,                   // wdExportDocumentWithMarkup
                    IncludeDocProps: true,
                    KeepIRM: true,
                    CreateBookmarks: 0,
                    DocStructureTags: true,
                    BitmapMissingFonts: true,
                    UseISO19005_1: false);
            }
            finally
            {
                doc.Close(SaveChanges: 0); // wdDoNotSaveChanges
            }
        }
        finally
        {
            word.Quit(SaveChanges: 0);
        }
    }
}
