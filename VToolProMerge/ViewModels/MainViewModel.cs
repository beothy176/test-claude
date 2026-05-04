using System.Collections.ObjectModel;
using System.Linq;
using VToolProMerge.Helpers;
using VToolProMerge.Models;
using VToolProMerge.Services;

namespace VToolProMerge.ViewModels;

public class MainViewModel : ViewModelBase
{
    private object? _currentView;
    private string _statusText = "Sẵn sàng";
    private string _searchQuery = string.Empty;
    private string _currentProfile = "Hồ sơ dự án ABC";
    private string _currentItem = "Báo giá tiêu chuẩn.docx";
    private string _storagePath = @"D:\VToolProMerge\Data\ProjectABC";
    private string _appVersion = "v2.1.0.0";
    private double _storageUsagePercent = 18.7;

    private readonly TemplateLibraryViewModel _templateLib;
    private readonly BatchMergeViewModel _batchMerge;
    private readonly DesignerViewModel _designer;
    private readonly PlaceholderManagerViewModel _placeholderMgr;
    private readonly CatalogViewModel _catalog;
    private readonly HistoryViewModel _history;
    private readonly DataSourceViewModel _dataSource;
    private readonly ProfileViewModel _profile;
    private readonly SettingsViewModel _settings;

    public MainViewModel()
    {
        // Khởi tạo services khung. Khi cần kết nối Word/Excel thật, inject ở đây.
        var logService = new AuditLogService();
        var templateScanner = new TemplateScanner();
        var placeholderSvc = new PlaceholderManagerService();
        var seed = SeedData.Build();

        _templateLib = new TemplateLibraryViewModel(seed.Templates);
        _batchMerge = new BatchMergeViewModel(seed.Templates, seed.Issues, seed.OutputFiles, seed.Logs);
        _designer = new DesignerViewModel(seed.DataSourceTree, seed.Elements);
        _placeholderMgr = new PlaceholderManagerViewModel(seed.Placeholders);
        _catalog = new CatalogViewModel();
        _history = new HistoryViewModel(seed.Logs);
        _dataSource = new DataSourceViewModel();
        _profile = new ProfileViewModel();
        _settings = new SettingsViewModel();

        Profiles = new ObservableCollection<string>
        {
            "Hồ sơ dự án ABC",
            "Hồ sơ dự án XYZ",
            "Hồ sơ thầu Q4/2025"
        };
        CurrentItems = new ObservableCollection<string>
        {
            "Báo giá tiêu chuẩn.docx",
            "Hợp đồng mua bán.docx",
            "Gói hồ sơ đấu thầu số 01"
        };

        NavItems = new ObservableCollection<NavItem>
        {
            new() { Key = "profile",     Title = "Hồ sơ",          Icon = "👤" },
            new() { Key = "templates",   Title = "Mẫu",            Icon = "📄", IsSelected = true },
            new() { Key = "catalog",     Title = "Danh mục",       Icon = "📂" },
            new() { Key = "placeholder", Title = "Placeholder",    Icon = "{ }" },
            new() { Key = "datasource",  Title = "Nguồn dữ liệu",  Icon = "🗄" },
            new() { Key = "batch",       Title = "Trộn bộ hồ sơ",  Icon = "👥" },
            new() { Key = "history",     Title = "Lịch sử",        Icon = "🕒" },
            new() { Key = "settings",    Title = "Cài đặt",        Icon = "⚙" },
        };

        CurrentView = _templateLib;

        NavigateCommand = new RelayCommand(p =>
        {
            if (p is NavItem nav) Navigate(nav.Key);
        });

        PreviewCommand = new RelayCommand(_ => DialogHelper.Info(
            "Tính năng Xem thử sẽ render bằng Microsoft Word thật ở phần engine.",
            "Xem thử"));
        CheckErrorsCommand = new RelayCommand(_ => DialogHelper.Info(
            "Bộ kiểm tra lỗi sẽ chạy ErrorCheckerService và liệt kê các vấn đề.",
            "Kiểm tra lỗi"));
        GenerateCommand = new RelayCommand(_ =>
        {
            Navigate("batch");
            DialogHelper.Info(
                "Hãy nhấn nút 'Sinh bộ hồ sơ' bên trong màn Trộn bộ hồ sơ để chạy BatchMergeService.",
                "Sinh hồ sơ");
        });
    }

    // ========== TOPBAR ==========
    public ObservableCollection<string> Profiles { get; }
    public ObservableCollection<string> CurrentItems { get; }

    public string CurrentProfile { get => _currentProfile; set => SetProperty(ref _currentProfile, value); }
    public string CurrentItem { get => _currentItem; set => SetProperty(ref _currentItem, value); }
    public string SearchQuery { get => _searchQuery; set => SetProperty(ref _searchQuery, value); }
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    // ========== STATUSBAR ==========
    public string StoragePath { get => _storagePath; set => SetProperty(ref _storagePath, value); }
    public string AppVersion { get => _appVersion; set => SetProperty(ref _appVersion, value); }
    public double StorageUsagePercent { get => _storageUsagePercent; set => SetProperty(ref _storageUsagePercent, value); }
    public string StorageUsageText => $"18.7 GB / 100 GB ({_storageUsagePercent:0.0}%)";

    // ========== NAV ==========
    public ObservableCollection<NavItem> NavItems { get; }
    public RelayCommand NavigateCommand { get; }
    public RelayCommand PreviewCommand { get; }
    public RelayCommand CheckErrorsCommand { get; }
    public RelayCommand GenerateCommand { get; }

    public object? CurrentView { get => _currentView; set => SetProperty(ref _currentView, value); }

    public void Navigate(string key)
    {
        CurrentView = key switch
        {
            "templates"   => _templateLib,
            "batch"       => _batchMerge,
            "placeholder" => _placeholderMgr,
            "catalog"     => _catalog,
            "history"     => _history,
            "datasource"  => _dataSource,
            "profile"     => _profile,
            "settings"    => _settings,
            _             => _templateLib
        };

        foreach (var n in NavItems) n.IsSelected = n.Key == key;
    }

    /// <summary>Cho phép mở Designer khi user chọn 1 mẫu cụ thể.</summary>
    public void OpenDesigner()
    {
        CurrentView = _designer;
        foreach (var n in NavItems) n.IsSelected = n.Key == "templates";
    }
}
