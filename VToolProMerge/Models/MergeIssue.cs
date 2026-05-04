namespace VToolProMerge.Models;

public enum IssueSeverity
{
    Info,
    Warning,
    Error
}

public enum IssueKind
{
    MissingPlaceholder,
    UnmappedData,
    DynamicTableMissingColumn,
    MissingTemplateFile,
    MissingExcelSheet
}

public class MergeIssue
{
    public IssueKind Kind { get; set; }
    public IssueSeverity Severity { get; set; } = IssueSeverity.Warning;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ActionLabel { get; set; } = "Chi tiết";
}
