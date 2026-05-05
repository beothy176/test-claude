using System.Collections.ObjectModel;
using VToolProMerge.Helpers;

namespace VToolProMerge.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _storagePath = @"D:\VToolProMerge\Data";
    private string _outputPath = @"D:\VToolProMerge\Output";
    private string _defaultFont = "Times New Roman";
    private int _defaultFontSize = 13;
    private string _exportFormat = "DOCX";
    private string _placeholderStyle = "Bracket [TEN]";
    private bool _useOfficeInterop;
    private bool _auditLogEnabled = true;
    private bool _confirmBeforeDelete = true;
    private bool _checkUpdates = true;
    private string _language = "Tiếng Việt";

    public string Title { get; } = "Cài đặt";
    public string Description { get; } =
        "Cấu hình đường dẫn lưu trữ, font mặc định, header/footer mẫu, tích hợp Office Interop, ...";

    public string StoragePath { get => _storagePath; set => SetProperty(ref _storagePath, value); }
    public string OutputPath { get => _outputPath; set => SetProperty(ref _outputPath, value); }
    public string DefaultFont { get => _defaultFont; set => SetProperty(ref _defaultFont, value); }
    public int DefaultFontSize { get => _defaultFontSize; set => SetProperty(ref _defaultFontSize, value); }
    public string ExportFormat { get => _exportFormat; set => SetProperty(ref _exportFormat, value); }
    public string PlaceholderStyle { get => _placeholderStyle; set => SetProperty(ref _placeholderStyle, value); }
    public bool UseOfficeInterop { get => _useOfficeInterop; set => SetProperty(ref _useOfficeInterop, value); }
    public bool AuditLogEnabled { get => _auditLogEnabled; set => SetProperty(ref _auditLogEnabled, value); }
    public bool ConfirmBeforeDelete { get => _confirmBeforeDelete; set => SetProperty(ref _confirmBeforeDelete, value); }
    public bool CheckUpdates { get => _checkUpdates; set => SetProperty(ref _checkUpdates, value); }
    public string Language { get => _language; set => SetProperty(ref _language, value); }

    public ObservableCollection<string> FontOptions { get; } = new()
    {
        "Times New Roman", "Arial", "Calibri", "Tahoma",
        "Roboto", "Open Sans", "Segoe UI"
    };
    public ObservableCollection<string> FontSizeOptions { get; } = new()
    {
        "10", "11", "12", "13", "14", "16"
    };
    public ObservableCollection<string> ExportFormatOptions { get; } = new()
    {
        "DOCX", "PDF", "DOCX+PDF"
    };
    public ObservableCollection<string> PlaceholderStyleOptions { get; } = new()
    {
        "Bracket [TEN]", "Curly {TEN}", "Mustache {{TEN}}", "Excel %TEN%"
    };
    public ObservableCollection<string> LanguageOptions { get; } = new()
    {
        "Tiếng Việt", "English"
    };

    public RelayCommand BrowseStorageCommand { get; }
    public RelayCommand BrowseOutputCommand { get; }
    public RelayCommand ApplyCommand { get; }
    public RelayCommand ResetCommand { get; }

    public SettingsViewModel()
    {
        BrowseStorageCommand = new RelayCommand(_ =>
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog();
            if (dlg.ShowDialog() == true) StoragePath = dlg.FolderName;
        });
        BrowseOutputCommand = new RelayCommand(_ =>
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog();
            if (dlg.ShowDialog() == true) OutputPath = dlg.FolderName;
        });
        ApplyCommand = new RelayCommand(_ =>
            DialogHelper.Info("Đã lưu cấu hình.\n\n(Các thiết lập sẽ áp dụng cho phiên làm việc kế tiếp.)",
                "Cài đặt"));
        ResetCommand = new RelayCommand(_ =>
        {
            if (!DialogHelper.Confirm("Khôi phục toàn bộ cài đặt về mặc định?",
                "Đặt lại cài đặt")) return;
            StoragePath = @"D:\VToolProMerge\Data";
            OutputPath = @"D:\VToolProMerge\Output";
            DefaultFont = "Times New Roman";
            DefaultFontSize = 13;
            ExportFormat = "DOCX";
            PlaceholderStyle = "Bracket [TEN]";
            UseOfficeInterop = false;
            AuditLogEnabled = true;
            ConfirmBeforeDelete = true;
            CheckUpdates = true;
            Language = "Tiếng Việt";
        });
    }
}
