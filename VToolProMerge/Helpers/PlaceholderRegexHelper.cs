using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VToolProMerge.Helpers;

/// <summary>
/// Placeholder dạng [TEN_TRUONG] — chữ in hoa, số và dấu gạch dưới.
/// Không dùng dạng { } theo quy ước phần mềm.
/// </summary>
public static class PlaceholderRegexHelper
{
    public const string PlaceholderPattern = @"\[([A-Z][A-Z0-9_]{0,63})\]";

    private static readonly Regex Rx = new(PlaceholderPattern, RegexOptions.Compiled);

    public static IEnumerable<string> Extract(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        foreach (Match m in Rx.Matches(text))
        {
            yield return m.Groups[1].Value;
        }
    }

    public static bool IsValid(string placeholder)
    {
        if (string.IsNullOrWhiteSpace(placeholder)) return false;
        return Regex.IsMatch(placeholder, @"^\[?[A-Z][A-Z0-9_]{0,63}\]?$");
    }

    public static string Wrap(string fieldName) => $"[{fieldName}]";
}
