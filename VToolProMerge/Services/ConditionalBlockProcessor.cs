using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using VToolProMerge.Helpers;

namespace VToolProMerge.Services;

/// <summary>
/// Xử lý khối điều kiện trong Word ở mức paragraph.
/// Cú pháp:
///     [IF:FIELD]            — bắt đầu khối, hiện khi FIELD truthy
///     [IF:FIELD=VALUE]      — hiện khi FIELD == VALUE
///     [IF:FIELD!=VALUE]     — hiện khi FIELD != VALUE
///     [ENDIF:FIELD]         — đóng khối tương ứng
/// "Truthy" = không rỗng, không phải "false"/"0"/"no"/"không".
/// Marker phải đứng độc lập trên paragraph (không trộn lẫn nội dung).
/// Khi điều kiện sai: xóa toàn bộ paragraph từ IF -> ENDIF (cả 2 marker).
/// Khi điều kiện đúng: chỉ xóa 2 paragraph chứa marker.
/// </summary>
public class ConditionalBlockProcessor
{
    private static readonly Regex IfRegex = new(
        @"\[IF:([A-Z][A-Z0-9_]+)(?:(=|!=)([^\]]*))?\]",
        RegexOptions.Compiled);
    private static readonly Regex EndIfRegex = new(
        @"\[ENDIF:([A-Z][A-Z0-9_]+)\]",
        RegexOptions.Compiled);

    public bool EvaluateCondition(string field, string op, string value,
        IReadOnlyDictionary<string, string?> data)
    {
        var token = $"[{field}]";
        data.TryGetValue(token, out var actual);
        actual ??= string.Empty;

        return op switch
        {
            "="  => string.Equals(actual, value, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(actual, value, StringComparison.OrdinalIgnoreCase),
            ""   => IsTruthy(actual),
            _    => false
        };
    }

    public void ApplyConditionalBlocks(string docxPath, IReadOnlyDictionary<string, string?> data)
    {
        using var doc = WordprocessingDocument.Open(docxPath, true);
        foreach (var part in OpenXmlHelpers.AllStoryParts(doc))
        {
            var root = OpenXmlHelpers.GetRoot(part);
            if (root == null) continue;
            ProcessElement(root, data);
        }
        doc.MainDocumentPart!.Document.Save();
    }

    private void ProcessElement(OpenXmlElement root, IReadOnlyDictionary<string, string?> data)
    {
        // Lặp đến khi không còn cặp IF/ENDIF nào cần xử lý.
        bool changed;
        do
        {
            changed = false;
            var paragraphs = root.Descendants<Paragraph>().ToList();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                var text = OpenXmlHelpers.ParagraphText(paragraphs[i]);
                var ifMatch = IfRegex.Match(text);
                if (!ifMatch.Success) continue;

                var field = ifMatch.Groups[1].Value;
                var op = ifMatch.Groups[2].Value;
                var val = ifMatch.Groups[3].Value;

                // Tìm ENDIF tương ứng (lồng nhau hỗ trợ qua đếm balance).
                int balance = 1;
                int endIdx = -1;
                for (int j = i + 1; j < paragraphs.Count; j++)
                {
                    var t = OpenXmlHelpers.ParagraphText(paragraphs[j]);
                    if (IfRegex.IsMatch(t) && IfMatchesField(t, field)) balance++;
                    var em = EndIfRegex.Match(t);
                    if (em.Success && em.Groups[1].Value == field)
                    {
                        balance--;
                        if (balance == 0) { endIdx = j; break; }
                    }
                }
                if (endIdx < 0) break; // cấu trúc lỗi — bỏ qua

                bool keep = EvaluateCondition(field, op, val, data);
                var startP = paragraphs[i];
                var endP = paragraphs[endIdx];

                if (keep)
                {
                    // Giữ nội dung, chỉ xóa 2 paragraph chứa marker.
                    startP.Remove();
                    endP.Remove();
                }
                else
                {
                    // Xóa tất cả paragraph từ i -> endIdx (cùng cấp).
                    // Chỉ xóa được khi cùng parent. Trường hợp lồng vào table cell etc.
                    // sẽ xóa từng paragraph bằng Remove() trực tiếp.
                    for (int k = i; k <= endIdx; k++)
                        paragraphs[k].Remove();
                }

                changed = true;
                break; // làm lại từ đầu để lấy danh sách paragraph mới
            }
        } while (changed);
    }

    private static bool IfMatchesField(string text, string field)
    {
        var m = IfRegex.Match(text);
        return m.Success && m.Groups[1].Value == field;
    }

    private static bool IsTruthy(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return false;
        var s = v.Trim().ToLowerInvariant();
        return s != "false" && s != "0" && s != "no" && s != "không" && s != "khong";
    }
}
