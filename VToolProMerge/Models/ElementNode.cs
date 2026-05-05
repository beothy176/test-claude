namespace VToolProMerge.Models;

public enum ElementKind
{
    Field,
    TableList,
    IfCondition,
    HeaderFooter,
    DynamicImage,
    Subdocument
}

public class ElementNode
{
    public string Name { get; set; } = string.Empty;
    public ElementKind Kind { get; set; }
    public string Icon { get; set; } = "🔤";
    public string Description { get; set; } = string.Empty;
}
