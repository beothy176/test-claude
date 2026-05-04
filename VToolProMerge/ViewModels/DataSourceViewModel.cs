using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using VToolProMerge.Helpers;
using VToolProMerge.Services;

namespace VToolProMerge.ViewModels;

public class DataSourceViewModel : ViewModelBase
{
    private readonly DataSourceManager _manager;
    private DataSourceEntry? _selected;
    private DataView? _selectedTableView;
    private string _selectedTableName = string.Empty;

    public DataSourceViewModel(DataSourceManager manager)
    {
        _manager = manager;
        Entries = manager.Entries;
        Scalars = new ObservableCollection<ScalarRow>();
        Tables = new ObservableCollection<string>();

        AddJsonCommand = new RelayCommand(_ => AddJson());
        AddExcelCommand = new RelayCommand(_ => AddExcel());
        AddSqliteCommand = new RelayCommand(_ => AddSqlite());
        RemoveCommand = new RelayCommand(_ =>
        {
            if (Selected != null) _manager.Remove(Selected);
        });
        SelectTableCommand = new RelayCommand(p =>
        {
            if (p is string name && Selected != null) ShowTable(name);
        });
        BuildContextCommand = new RelayCommand(_ =>
        {
            var ctx = _manager.BuildContext();
            DialogHelper.Info(
                $"MergeContext sẵn sàng:\n  • {ctx.Tokens.Count} field scalar\n" +
                $"  • {ctx.TableSources.Count} bảng dữ liệu\n\n" +
                "Có thể sang màn 'Trộn bộ hồ sơ' để Sinh hồ sơ.",
                "Build MergeContext");
        });
    }

    public ObservableCollection<DataSourceEntry> Entries { get; }
    public ObservableCollection<ScalarRow> Scalars { get; }
    public ObservableCollection<string> Tables { get; }

    public DataSourceEntry? Selected
    {
        get => _selected;
        set { if (SetProperty(ref _selected, value)) ReloadDetail(); }
    }

    public string SelectedTableName { get => _selectedTableName; set => SetProperty(ref _selectedTableName, value); }
    public DataView? SelectedTableView { get => _selectedTableView; set => SetProperty(ref _selectedTableView, value); }

    public RelayCommand AddJsonCommand { get; }
    public RelayCommand AddExcelCommand { get; }
    public RelayCommand AddSqliteCommand { get; }
    public RelayCommand RemoveCommand { get; }
    public RelayCommand SelectTableCommand { get; }
    public RelayCommand BuildContextCommand { get; }

    private void AddJson()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON|*.json",
            Title = "Chọn file JSON dữ liệu"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var src = new JsonDataSource(dlg.FileName);
            var e = _manager.Add(src, "JSON", dlg.FileName);
            Selected = e;
        }
        catch (System.Exception ex) { DialogHelper.Error(ex.Message); }
    }

    private void AddExcel()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Excel|*.xlsx;*.xlsm",
            Title = "Chọn file Excel dữ liệu"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var src = new ExcelDataSource(dlg.FileName);
            var e = _manager.Add(src, "Excel", dlg.FileName);
            Selected = e;
        }
        catch (System.Exception ex) { DialogHelper.Error(ex.Message); }
    }

    private void AddSqlite()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "SQLite|*.db;*.sqlite;*.sqlite3",
            Title = "Chọn file SQLite"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            // TODO: trong production cho user chọn whitelist bảng. Ở đây tạm dùng tất cả.
            var src = new SqliteDataSource(dlg.FileName, GuessTables(dlg.FileName));
            var e = _manager.Add(src, "SQLite", dlg.FileName);
            Selected = e;
        }
        catch (System.Exception ex) { DialogHelper.Error(ex.Message); }
    }

    private static IEnumerable<string> GuessTables(string dbPath)
    {
        try
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name <> 'Scalars'";
            using var rdr = cmd.ExecuteReader();
            var list = new List<string>();
            while (rdr.Read()) list.Add(rdr.GetString(0));
            return list;
        }
        catch { return System.Array.Empty<string>(); }
    }

    private void ReloadDetail()
    {
        Scalars.Clear();
        Tables.Clear();
        SelectedTableView = null;
        SelectedTableName = string.Empty;
        if (_selected == null) return;

        foreach (var kv in _selected.Source.GetScalars().OrderBy(k => k.Key))
            Scalars.Add(new ScalarRow(kv.Key, kv.Value?.ToString() ?? string.Empty));
        foreach (var k in _selected.Source.GetTables().Keys.OrderBy(k => k))
            Tables.Add(k);

        if (Tables.Count > 0) ShowTable(Tables[0]);
    }

    private void ShowTable(string name)
    {
        if (_selected == null) return;
        if (!_selected.Source.GetTables().TryGetValue(name, out var dt)) return;
        SelectedTableName = name;
        SelectedTableView = dt.DefaultView;
    }

    public class ScalarRow
    {
        public ScalarRow(string key, string value) { Key = key; Value = value; }
        public string Key { get; }
        public string Value { get; }
        public string Token => $"[{Key}]";
    }
}
