using System;

namespace VToolProMerge.Models;

public enum TemplateKind
{
    Word,
    Excel
}

public class TemplateItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;        // Hợp đồng, Báo giá, ...
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "v1.0.0";
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string Author { get; set; } = string.Empty;
    public TemplateKind Kind { get; set; } = TemplateKind.Word;
    public string FilePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Format => Kind == TemplateKind.Excel ? ".xlsx" : ".docx";
    public DateTime? LastUsedAt { get; set; }
    public int UseCount { get; set; }

    public int PlaceholderCount { get; set; }
    public int DynamicTableCount { get; set; }
    public int ConditionCount { get; set; }
    public string Status { get; set; } = "Sẵn sàng"; // "Sẵn sàng" | "Cảnh báo" | "Lỗi"

    public string SizeText => SizeBytes < 1024
        ? $"{SizeBytes} B"
        : SizeBytes < 1024 * 1024
            ? $"{SizeBytes / 1024} KB"
            : $"{SizeBytes / 1024 / 1024} MB";
}
