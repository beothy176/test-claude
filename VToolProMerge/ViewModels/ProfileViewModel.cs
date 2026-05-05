using System;
using System.Collections.ObjectModel;
using System.Linq;
using VToolProMerge.Helpers;

namespace VToolProMerge.ViewModels;

public class ProfileItem : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _dataSource = string.Empty;
    private int _templateCount;
    private int _historyCount;
    private int _outputCount;
    private DateTime _modifiedAt = DateTime.Now;
    private bool _isActive;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string DataSource { get => _dataSource; set => SetProperty(ref _dataSource, value); }
    public int TemplateCount { get => _templateCount; set => SetProperty(ref _templateCount, value); }
    public int HistoryCount { get => _historyCount; set => SetProperty(ref _historyCount, value); }
    public int OutputCount { get => _outputCount; set => SetProperty(ref _outputCount, value); }
    public DateTime ModifiedAt
    {
        get => _modifiedAt;
        set { SetProperty(ref _modifiedAt, value); OnPropertyChanged(nameof(ModifiedAtText)); }
    }
    public bool IsActive
    {
        get => _isActive;
        set { SetProperty(ref _isActive, value); OnPropertyChanged(nameof(StatusText)); }
    }

    public string ModifiedAtText => _modifiedAt.ToString("dd/MM/yyyy HH:mm");
    public string StatusText => _isActive ? "Đang dùng" : "Sẵn sàng";
}

public class ProfileViewModel : ViewModelBase
{
    private readonly Action<string>? _navigate;
    private ProfileItem? _selected;

    public string Title { get; } = "Hồ sơ";
    public string Description { get; } =
        "Quản lý các hồ sơ làm việc. Mỗi hồ sơ giữ nguồn dữ liệu, mẫu, lịch sử và file đã sinh ra.";

    public ObservableCollection<ProfileItem> Profiles { get; }

    public ProfileItem? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand RenameCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ActivateCommand { get; }
    public RelayCommand OpenTemplatesCommand { get; }
    public RelayCommand OpenHistoryCommand { get; }

    public ProfileViewModel(Action<string>? navigate = null)
    {
        _navigate = navigate;

        Profiles = new ObservableCollection<ProfileItem>
        {
            new() { Name = "Hồ sơ dự án ABC", Description = "Dự án xây lắp ABC, gói số 01",
                    DataSource = "ProjectABC.xlsx", TemplateCount = 8, HistoryCount = 24,
                    OutputCount = 162, IsActive = true,
                    ModifiedAt = DateTime.Now.AddHours(-2) },
            new() { Name = "Hồ sơ dự án XYZ", Description = "Dự án tư vấn XYZ Q4/2025",
                    DataSource = "ProjectXYZ.xlsx", TemplateCount = 5, HistoryCount = 11,
                    OutputCount = 47, ModifiedAt = DateTime.Now.AddDays(-3) },
            new() { Name = "Hồ sơ thầu Q4/2025", Description = "Gói thầu mua sắm thiết bị",
                    DataSource = "BiddingQ4.xlsx", TemplateCount = 12, HistoryCount = 6,
                    OutputCount = 72, ModifiedAt = DateTime.Now.AddDays(-7) },
            new() { Name = "Hồ sơ dân sinh 2026", Description = "Hồ sơ chính sách dân sinh",
                    DataSource = "(chưa chọn)", TemplateCount = 0, HistoryCount = 0,
                    OutputCount = 0, ModifiedAt = DateTime.Now.AddDays(-30) },
        };
        Selected = Profiles.FirstOrDefault();

        AddCommand = new RelayCommand(_ =>
        {
            var p = new ProfileItem
            {
                Name = $"Hồ sơ mới {Profiles.Count + 1}",
                Description = "(chưa có mô tả)",
                DataSource = "(chưa chọn)",
                ModifiedAt = DateTime.Now,
            };
            Profiles.Add(p);
            Selected = p;
        });

        RenameCommand = new RelayCommand(_ =>
        {
            if (Selected == null) return;
            DialogHelper.Info(
                "Đổi tên hồ sơ — sẽ mở dialog InputBox.\n\n" +
                $"(Hiện tại: {Selected.Name})",
                "Đổi tên hồ sơ");
        });

        DeleteCommand = new RelayCommand(_ =>
        {
            if (Selected == null) return;
            if (Selected.IsActive)
            {
                DialogHelper.Warn("Không thể xoá hồ sơ đang dùng. Hãy chuyển sang hồ sơ khác trước.",
                    "Xoá hồ sơ");
                return;
            }
            if (!DialogHelper.Confirm($"Xoá hồ sơ '{Selected.Name}'? Thao tác không thể hoàn tác.",
                "Xác nhận xoá")) return;
            var idx = Profiles.IndexOf(Selected);
            Profiles.Remove(Selected);
            Selected = idx < Profiles.Count ? Profiles[idx] : Profiles.LastOrDefault();
        });

        ActivateCommand = new RelayCommand(_ =>
        {
            if (Selected == null) return;
            foreach (var p in Profiles) p.IsActive = false;
            Selected.IsActive = true;
            Selected.ModifiedAt = DateTime.Now;
        });

        OpenTemplatesCommand = new RelayCommand(_ => _navigate?.Invoke("templates"));
        OpenHistoryCommand = new RelayCommand(_ => _navigate?.Invoke("history"));
    }
}
