using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VToolProMerge.Models;

public class BatchTemplateItem : INotifyPropertyChanged
{
    private bool _isSelected = true;
    private string _status = "Sẵn sàng"; // "Sẵn sàng" | "Cảnh báo" | "Lỗi"

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TemplateKind Kind { get; set; } = TemplateKind.Word;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnChange(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnChange(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChange([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
