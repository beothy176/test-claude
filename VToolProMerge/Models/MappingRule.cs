namespace VToolProMerge.Models;

public class MappingRule
{
    public string Placeholder { get; set; } = string.Empty;     // [TEN_TRUONG]
    public string DisplayName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;          // JSON | SQLite | Excel | Manual
    public string SourceField { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public bool Required { get; set; }
    public string? Note { get; set; }
}
