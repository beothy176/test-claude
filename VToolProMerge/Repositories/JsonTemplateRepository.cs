using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VToolProMerge.Models;

namespace VToolProMerge.Repositories;

public class JsonTemplateRepository : ITemplateRepository
{
    private readonly string _path;
    private List<TemplateItem> _items = new();

    public JsonTemplateRepository(string filePath)
    {
        _path = filePath;
        Load();
    }

    public IReadOnlyList<TemplateItem> GetAll() => _items;
    public TemplateItem? GetById(string id) => _items.FirstOrDefault(t => t.Id == id);

    public void Add(TemplateItem item) { _items.Add(item); Save(); }

    public void Update(TemplateItem item)
    {
        var idx = _items.FindIndex(t => t.Id == item.Id);
        if (idx >= 0) { _items[idx] = item; Save(); }
    }

    public void Delete(string id)
    {
        _items.RemoveAll(t => t.Id == id);
        Save();
    }

    private void Load()
    {
        if (!File.Exists(_path)) { _items = new(); return; }
        var json = File.ReadAllText(_path);
        _items = JsonSerializer.Deserialize<List<TemplateItem>>(json) ?? new();
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(_path, JsonSerializer.Serialize(_items,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
