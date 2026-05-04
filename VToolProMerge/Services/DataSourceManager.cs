using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VToolProMerge.Services;

/// <summary>
/// State chia sẻ giữa các ViewModel: danh sách IDataSource đã nạp.
/// Khi BatchMergeService cần MergeContext, nó gọi BuildContext() để gộp toàn bộ.
/// </summary>
public class DataSourceManager
{
    private readonly DataMappingEngine _mapping;
    public ObservableCollection<DataSourceEntry> Entries { get; } = new();

    public DataSourceManager(DataMappingEngine mapping)
    {
        _mapping = mapping;
    }

    public event EventHandler? Changed;

    public DataSourceEntry Add(IDataSource src, string kind, string path)
    {
        var e = new DataSourceEntry
        {
            Source = src,
            Kind = kind,
            Path = path,
            Name = src.Name
        };
        Entries.Add(e);
        Changed?.Invoke(this, EventArgs.Empty);
        return e;
    }

    public void Remove(DataSourceEntry e)
    {
        Entries.Remove(e);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool HasAny => Entries.Count > 0;

    public MergeContext BuildContext()
    {
        return _mapping.BuildContext(Entries.Select(e => e.Source));
    }
}

public class DataSourceEntry
{
    public IDataSource Source { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "JSON"; // JSON | Excel | SQLite
    public string Path { get; set; } = string.Empty;
    public int ScalarCount => Source?.GetScalars().Count ?? 0;
    public int TableCount => Source?.GetTables().Count ?? 0;
}
