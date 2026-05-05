using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VToolProMerge.Helpers;
using VToolProMerge.Models;

namespace VToolProMerge.ViewModels;

public class TemplateLibraryViewModel : ViewModelBase
{
    private readonly List<TemplateItem> _all;
    private TemplateItem? _selected;
    private string _activeCategory = "Tất cả";
    private string _searchQuery = string.Empty;
    private bool _isGridView = true;

    public TemplateLibraryViewModel(IEnumerable<TemplateItem> templates)
    {
        _all = templates.ToList();
        Items = new ObservableCollection<TemplateItem>(_all);
        Categories = new ObservableCollection<CategoryChip>
        {
            new("Tất cả", _all.Count),
            new("Đấu thầu", _all.Count(t => t.Category == "Đấu thầu")),
            new("Hợp đồng", _all.Count(t => t.Category == "Hợp đồng")),
            new("Báo giá", _all.Count(t => t.Category == "Báo giá")),
            new("Báo cáo", _all.Count(t => t.Category == "Báo cáo")),
            new("Quyết định", _all.Count(t => t.Category == "Quyết định")),
            new("Tờ trình", _all.Count(t => t.Category == "Tờ trình")),
        };
        Selected = _all.FirstOrDefault();

        FilterCommand = new RelayCommand(p =>
        {
            if (p is string c) { ActiveCategory = c; ApplyFilter(); }
        });
        SelectCommand = new RelayCommand(p =>
        {
            if (p is TemplateItem t) Selected = t;
        });
        ToggleViewCommand = new RelayCommand(p =>
        {
            if (p is string s) IsGridView = s == "grid";
        });
        DesignCommand = new RelayCommand(_ => DialogHelper.Info(
            "Mở Designer cho mẫu được chọn (chuyển sang màn Thiết kế mẫu).",
            "Designer"));
    }

    public ObservableCollection<TemplateItem> Items { get; }
    public ObservableCollection<CategoryChip> Categories { get; }

    public TemplateItem? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public string ActiveCategory
    {
        get => _activeCategory;
        set => SetProperty(ref _activeCategory, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set { if (SetProperty(ref _searchQuery, value)) ApplyFilter(); }
    }

    public bool IsGridView
    {
        get => _isGridView;
        set { SetProperty(ref _isGridView, value); OnPropertyChanged(nameof(IsListView)); }
    }

    public bool IsListView => !_isGridView;

    public RelayCommand FilterCommand { get; }
    public RelayCommand SelectCommand { get; }
    public RelayCommand ToggleViewCommand { get; }
    public RelayCommand DesignCommand { get; }

    private void ApplyFilter()
    {
        Items.Clear();
        var q = (_searchQuery ?? string.Empty).Trim().ToLowerInvariant();
        IEnumerable<TemplateItem> src = _all;
        if (_activeCategory != "Tất cả")
            src = src.Where(t => t.Category == _activeCategory);
        if (!string.IsNullOrEmpty(q))
            src = src.Where(t =>
                t.Name.ToLowerInvariant().Contains(q) ||
                t.Description.ToLowerInvariant().Contains(q));
        foreach (var t in src) Items.Add(t);
        if (Selected == null || !Items.Contains(Selected))
            Selected = Items.FirstOrDefault();
    }

    public class CategoryChip
    {
        public CategoryChip(string name, int count) { Name = name; Count = count; }
        public string Name { get; }
        public int Count { get; }
        public string Display => $"{Name} ({Count})";
    }
}
