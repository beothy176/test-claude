namespace VToolProMerge.Models;

public class OutputFileItem
{
    public string FileName { get; set; } = string.Empty;
    public string Format { get; set; } = "DOCX";
    public string SizeText { get; set; } = string.Empty;
    public string Status { get; set; } = "Sẵn sàng"; // Sẵn sàng | Cảnh báo | Lỗi
    public string? FullPath { get; set; }
}
