using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;
using VToolProMerge.Helpers;
using VToolProMerge.Models;
using VToolProMerge.Services;

namespace VToolProMerge.ViewModels;

public class PlaceholderManagerViewModel : ViewModelBase
{
    private readonly PlaceholderManagerService _service;
    private string _filterGroup = "Tất cả";
    private string _searchQuery = string.Empty;
    private readonly List<PlaceholderItem> _all;

    public PlaceholderManagerViewModel(IEnumerable<PlaceholderItem> seed)
    {
        _service = new PlaceholderManagerService();
        _all = seed.ToList();
        foreach (var p in _all) _service.AddPlaceholder(p);

        Items = new ObservableCollection<PlaceholderItem>(_all);
        Groups = new ObservableCollection<string>(new[] { "Tất cả" }
            .Concat(_all.Select(p => p.Group).Distinct().OrderBy(g => g)));

        AddCommand = new RelayCommand(_ => AddNew());
        DeleteCommand = new RelayCommand(p =>
        {
            if (p is PlaceholderItem item)
            {
                _service.DeletePlaceholder(item.Id);
                _all.RemoveAll(x => x.Id == item.Id);
                ApplyFilter();
            }
        });
        ImportCommand = new RelayCommand(_ => Import());
        ExportCommand = new RelayCommand(_ => Export());
        FilterCommand = new RelayCommand(p => { if (p is string g) { FilterGroup = g; ApplyFilter(); } });
    }

    public ObservableCollection<PlaceholderItem> Items { get; }
    public ObservableCollection<string> Groups { get; }

    public string FilterGroup
    {
        get => _filterGroup;
        set => SetProperty(ref _filterGroup, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set { if (SetProperty(ref _searchQuery, value)) ApplyFilter(); }
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand FilterCommand { get; }

    public int TotalCount => _all.Count;
    public int RequiredCount => _all.Count(p => p.Required);
    public int GroupCount => _all.Select(p => p.Group).Distinct().Count();

    private void ApplyFilter()
    {
        Items.Clear();
        IEnumerable<PlaceholderItem> q = _all;
        if (FilterGroup != "Tất cả")
            q = q.Where(p => p.Group == FilterGroup);
        if (!string.IsNullOrWhiteSpace(SearchQuery))
            q = q.Where(p =>
                p.FieldName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase) ||
                p.DisplayName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase));
        foreach (var p in q) Items.Add(p);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(RequiredCount));
        OnPropertyChanged(nameof(GroupCount));
    }

    private void AddNew()
    {
        var item = new PlaceholderItem
        {
            FieldName = "FIELD_NEW",
            DisplayName = "Trường mới",
            Group = FilterGroup == "Tất cả" ? "Khác" : FilterGroup
        };
        _service.AddPlaceholder(item);
        _all.Add(item);
        ApplyFilter();
    }

    private void Import()
    {
        var dlg = new OpenFileDialog { Filter = "JSON|*.json", Title = "Import placeholder" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var json = File.ReadAllText(dlg.FileName);
            var loaded = JsonSerializer.Deserialize<List<PlaceholderItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (loaded == null) return;

            var existing = _all.Select(p => p.FieldName).ToHashSet();
            int added = 0;
            foreach (var p in loaded)
            {
                if (PlaceholderRegexHelper.IsValid(p.FieldName) && !existing.Contains(p.FieldName))
                {
                    _service.AddPlaceholder(p);
                    _all.Add(p);
                    existing.Add(p.FieldName);
                    added++;
                }
            }
            ApplyFilter();
            DialogHelper.Info($"Đã import {added}/{loaded.Count} placeholder.", "Import");
        }
        catch (System.Exception ex) { DialogHelper.Error(ex.Message); }
    }

    private void Export()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "JSON|*.json",
            FileName = "placeholders.json",
            Title = "Export placeholder"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var json = JsonSerializer.Serialize(_all,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dlg.FileName, json);
            DialogHelper.Info($"Đã xuất {_all.Count} placeholder.", "Export");
        }
        catch (System.Exception ex) { DialogHelper.Error(ex.Message); }
    }
}
