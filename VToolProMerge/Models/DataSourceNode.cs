using System.Collections.ObjectModel;

namespace VToolProMerge.Models;

public class DataSourceNode
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public string Type { get; set; } = "Group"; // Group | Field | Table
    public bool IsUsed { get; set; }
    public bool HasData { get; set; } = true;
    public ObservableCollection<DataSourceNode> Children { get; } = new();
}
