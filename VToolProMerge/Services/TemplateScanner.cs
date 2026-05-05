using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using VToolProMerge.Helpers;

namespace VToolProMerge.Services;

/// <summary>
/// Quét placeholder dạng [TEN_TRUONG] trong file DOCX hoặc text.
/// Bản hiện tại đọc text trong tất cả paragraph + bảng + header/footer.
/// </summary>
public class TemplateScanner
{
    public IReadOnlyList<string> ScanDocxPlaceholders(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return Array.Empty<string>();

        var found = new HashSet<string>(StringComparer.Ordinal);

        // TODO: khi có file Word thật, mở rộng để gom Run bị tách (split runs).
        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return Array.Empty<string>();

        var text = body.InnerText;
        foreach (var name in PlaceholderRegexHelper.Extract(text))
            found.Add(name);

        return new List<string>(found);
    }

    public IReadOnlyList<string> ScanTextPlaceholders(string text)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var n in PlaceholderRegexHelper.Extract(text)) set.Add(n);
        return new List<string>(set);
    }

    public bool ValidatePlaceholderFormat(string placeholder)
        => PlaceholderRegexHelper.IsValid(placeholder);
}
