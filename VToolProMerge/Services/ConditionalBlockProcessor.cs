using System.Collections.Generic;

namespace VToolProMerge.Services;

/// <summary>
/// Xử lý khối điều kiện trong Word.
/// Cú pháp đề xuất (đặt trong content control hoặc bookmark): IF [HAS_VAT] = TRUE
/// </summary>
public class ConditionalBlockProcessor
{
    public bool EvaluateCondition(string field, string op, string value,
        IReadOnlyDictionary<string, string?> data)
    {
        data.TryGetValue(field, out var actual);
        return op switch
        {
            "="  => string.Equals(actual, value, System.StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(actual, value, System.StringComparison.OrdinalIgnoreCase),
            ">"  => decimal.TryParse(actual, out var a) && decimal.TryParse(value, out var b) && a > b,
            "<"  => decimal.TryParse(actual, out var a2) && decimal.TryParse(value, out var b2) && a2 < b2,
            _    => false
        };
    }

    public void ApplyConditionalBlocks(string docxPath, IReadOnlyDictionary<string, string?> data)
    {
        // TODO: Duyệt các content control / bookmark đánh dấu IF...ENDIF.
        // Nếu điều kiện sai thì xóa block; nếu đúng thì giữ và xóa marker.
    }
}
