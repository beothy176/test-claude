using System;

namespace VToolProMerge.Models;

public class PlaceholderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Tên field, không bao gồm ngoặc vuông. VD: NT_TEN.</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Token đầy đủ dạng [TEN_TRUONG].</summary>
    public string Token => $"[{FieldName}]";

    public string DisplayName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty; // Thông tin nhà thầu, Đơn vị, ...
    public string DataType { get; set; } = "Text";    // Text, Number, Date, Money, ...
    public string? DefaultValue { get; set; }
    public bool Required { get; set; }
    public string? Source { get; set; }               // Nguồn dữ liệu (vd: NhaThau, HangHoa)
    public string? SourceField { get; set; }
    public string Description { get; set; } = string.Empty;
}
