using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace VToolProMerge.Helpers;

/// <summary>
/// Tiện ích thao tác OpenXML cho Word: lấy toàn bộ text trong 1 paragraph
/// (Word thường tách 1 chuỗi thành nhiều Run/Text khi user gõ chậm hoặc paste),
/// thay placeholder mà giữ format của Run đầu, duyệt header/footer.
/// </summary>
public static class OpenXmlHelpers
{
    public static IEnumerable<OpenXmlPart> AllStoryParts(WordprocessingDocument doc)
    {
        var main = doc.MainDocumentPart;
        if (main == null) yield break;
        yield return main;
        foreach (var h in main.HeaderParts) yield return h;
        foreach (var f in main.FooterParts) yield return f;
        if (main.FootnotesPart != null) yield return main.FootnotesPart;
        if (main.EndnotesPart != null) yield return main.EndnotesPart;
    }

    /// <summary>Trả về root element của một story part (Document/Header/Footer/...).</summary>
    public static OpenXmlElement? GetRoot(OpenXmlPart part) => part switch
    {
        MainDocumentPart m => m.Document?.Body,
        HeaderPart h => h.Header,
        FooterPart f => f.Footer,
        FootnotesPart fn => fn.Footnotes,
        EndnotesPart en => en.Endnotes,
        _ => null
    };

    /// <summary>
    /// Thay placeholder trong toàn bộ paragraph của 1 element gốc (body/header/footer).
    /// Giữ định dạng của Run đầu chứa placeholder, ghép các Run bị tách lại.
    /// </summary>
    public static int ReplacePlaceholdersInElement(
        OpenXmlElement root,
        IReadOnlyDictionary<string, string?> values)
    {
        int count = 0;

        foreach (var p in root.Descendants<Paragraph>().ToList())
        {
            count += ReplaceInParagraph(p, values);
        }

        // Bảng có thể chứa paragraph rồi nên đã bao hàm trong Descendants<Paragraph>.
        return count;
    }

    public static int ReplaceInParagraph(Paragraph p, IReadOnlyDictionary<string, string?> values)
    {
        var texts = p.Descendants<Text>().ToList();
        if (texts.Count == 0) return 0;
        var full = string.Concat(texts.Select(t => t.Text));
        if (string.IsNullOrEmpty(full)) return 0;

        // Quick scan: paragraph có placeholder không?
        if (!Regex.IsMatch(full, PlaceholderRegexHelper.PlaceholderPattern)) return 0;

        var replaced = full;
        foreach (var (token, val) in values)
            replaced = replaced.Replace(token, val ?? string.Empty);

        if (replaced == full) return 0;

        // Đặt toàn bộ text mới vào Text đầu tiên, xóa phần text của Text còn lại.
        // Giữ Run/RunProperties của Run chứa Text đầu (định dạng phổ biến của paragraph).
        texts[0].Text = replaced;
        texts[0].Space = SpaceProcessingModeValues.Preserve;
        for (int i = 1; i < texts.Count; i++) texts[i].Text = string.Empty;
        return 1;
    }

    /// <summary>Lấy text gộp của paragraph (đã ghép các run/text bị tách).</summary>
    public static string ParagraphText(Paragraph p)
        => string.Concat(p.Descendants<Text>().Select(t => t.Text));

    /// <summary>Lấy text gộp của 1 row (gồm tất cả cell).</summary>
    public static string RowText(TableRow r)
        => string.Concat(r.Descendants<Text>().Select(t => t.Text));
}
