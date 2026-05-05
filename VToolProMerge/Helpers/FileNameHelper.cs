using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VToolProMerge.Helpers;

public static class FileNameHelper
{
    private static readonly Regex Token = new(@"\[([A-Z][A-Z0-9_]+)\]", RegexOptions.Compiled);

    /// <summary>
    /// Resolve template tên file dạng [SO_HOSO]_[TEN_GOI_THAU]_[TEN_MAU]
    /// </summary>
    public static string Resolve(string template, IReadOnlyDictionary<string, string?> values)
    {
        var resolved = Token.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            return values.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v) ? v! : "_";
        });
        return Sanitize(resolved);
    }

    public static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Trim();
    }
}
