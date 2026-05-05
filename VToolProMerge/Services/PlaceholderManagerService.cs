using System.Collections.Generic;
using System.Linq;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

public class PlaceholderManagerService
{
    private readonly List<PlaceholderItem> _store = new();

    public IReadOnlyList<PlaceholderItem> All => _store;

    public void AddPlaceholder(PlaceholderItem item) => _store.Add(item);

    public void UpdatePlaceholder(PlaceholderItem item)
    {
        var idx = _store.FindIndex(p => p.Id == item.Id);
        if (idx >= 0) _store[idx] = item;
    }

    public void DeletePlaceholder(string id)
        => _store.RemoveAll(p => p.Id == id);

    public void ImportPlaceholders(IEnumerable<PlaceholderItem> items)
        => _store.AddRange(items);

    public IEnumerable<PlaceholderItem> ExportPlaceholders() => _store;

    public IEnumerable<IGrouping<string, PlaceholderItem>> GroupByCategory()
        => _store.GroupBy(p => p.Group);
}
