using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VToolProMerge.Models;

public class NavItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnChange(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChange([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
