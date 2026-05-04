using System;

namespace VToolProMerge.Models;

public class AuditLogItem
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = "Hoàn tất";
    public string? Detail { get; set; }
    public string? Profile { get; set; }
    public string? User { get; set; }
}
