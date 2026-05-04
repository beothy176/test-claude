using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VToolProMerge.Helpers;
using VToolProMerge.Models;

namespace VToolProMerge.ViewModels;

public class BatchMergeViewModel : ViewModelBase
{
    private string _outputDir = @"D:\VToolProMerge\Output\HS_ABC_01";
    private string _fileNamePattern = "[SO_HOSO]_[TEN_GOI_THAU]_[TEN_MAU]";
    private string _exportFormat = "DOCX"; // DOCX | PDF | DOCX+PDF
    private bool _openFileAfter = true;
    private bool _openFolderAfter = true;
    private bool _saveHistory = true;
    private bool _writeDetailedLog;
    private bool _printAfter;

    private int _step1Done;
    private int _step2Done;
    private int _step3Done;
    private int _step4Done;
    private int _filesDone;
    private int _filesTotal = 8;

    public BatchMergeViewModel(
        IEnumerable<TemplateItem> seedTemplates,
        IEnumerable<MergeIssue> seedIssues,
        IEnumerable<OutputFileItem> seedOutputs,
        IEnumerable<AuditLogItem> seedLogs)
    {
        Templates = new ObservableCollection<BatchTemplateItem>(
            seedTemplates.Take(8).Select(t => new BatchTemplateItem
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category,
                Kind = t.Kind,
                Status = t.Status
            }));

        Issues = new ObservableCollection<MergeIssue>(seedIssues);
        OutputFiles = new ObservableCollection<OutputFileItem>(seedOutputs);
        Logs = new ObservableCollection<AuditLogItem>(seedLogs);

        SelectAllCommand = new RelayCommand(_ => { foreach (var t in Templates) t.IsSelected = true; });
        DeselectAllCommand = new RelayCommand(_ => { foreach (var t in Templates) t.IsSelected = false; });
        RecheckCommand = new RelayCommand(_ => DialogHelper.Info(
            "Bộ kiểm tra lỗi sẽ chạy lại ErrorCheckerService.", "Kiểm tra lại"));
        BrowseFolderCommand = new RelayCommand(_ =>
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog();
            if (dlg.ShowDialog() == true) OutputDir = dlg.FolderName;
        });
        AdvancedSettingsCommand = new RelayCommand(_ => DialogHelper.Info(
            "Mở thiết lập nâng cao (template tên file, font, header/footer mặc định, ...).",
            "Thiết lập nâng cao"));

        GenerateCommand = new RelayCommand(async _ => await RunGenerateAsync());
    }

    // ================ STAT ================
    public int SelectedCount => Templates.Count(t => t.IsSelected);
    public int PlaceholderCount => 24;
    public int DynamicTableCount => 3;
    public int ConditionCount => 2;
    public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);
    public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
    public string SelectedTotalText => $"{SelectedCount} / {Templates.Count}";

    // ================ DATA ================
    public ObservableCollection<BatchTemplateItem> Templates { get; }
    public ObservableCollection<MergeIssue> Issues { get; }
    public ObservableCollection<OutputFileItem> OutputFiles { get; }
    public ObservableCollection<AuditLogItem> Logs { get; }

    // ================ OUTPUT CONFIG ================
    public string OutputDir { get => _outputDir; set => SetProperty(ref _outputDir, value); }
    public string FileNamePattern { get => _fileNamePattern; set => SetProperty(ref _fileNamePattern, value); }
    public string ExportFormat
    {
        get => _exportFormat;
        set
        {
            if (SetProperty(ref _exportFormat, value))
            {
                OnPropertyChanged(nameof(ExportDocx));
                OnPropertyChanged(nameof(ExportPdf));
                OnPropertyChanged(nameof(ExportBoth));
            }
        }
    }
    public bool ExportDocx { get => _exportFormat == "DOCX";     set { if (value) ExportFormat = "DOCX"; } }
    public bool ExportPdf  { get => _exportFormat == "PDF";      set { if (value) ExportFormat = "PDF"; } }
    public bool ExportBoth { get => _exportFormat == "DOCX+PDF"; set { if (value) ExportFormat = "DOCX+PDF"; } }

    public bool OpenFileAfter { get => _openFileAfter; set => SetProperty(ref _openFileAfter, value); }
    public bool OpenFolderAfter { get => _openFolderAfter; set => SetProperty(ref _openFolderAfter, value); }
    public bool SaveHistory { get => _saveHistory; set => SetProperty(ref _saveHistory, value); }
    public bool WriteDetailedLog { get => _writeDetailedLog; set => SetProperty(ref _writeDetailedLog, value); }
    public bool PrintAfter { get => _printAfter; set => SetProperty(ref _printAfter, value); }

    // ================ PROGRESS ================
    public int FilesDone { get => _filesDone; set { SetProperty(ref _filesDone, value); OnPropertyChanged(nameof(ProgressText)); OnPropertyChanged(nameof(ProgressPercent)); } }
    public int FilesTotal { get => _filesTotal; set => SetProperty(ref _filesTotal, value); }
    public string ProgressText => $"{FilesDone} / {FilesTotal} file";
    public double ProgressPercent => FilesTotal == 0 ? 0 : (double)FilesDone * 100 / FilesTotal;

    public int Step1Done { get => _step1Done; set => SetProperty(ref _step1Done, value); }
    public int Step2Done { get => _step2Done; set => SetProperty(ref _step2Done, value); }
    public int Step3Done { get => _step3Done; set => SetProperty(ref _step3Done, value); }
    public int Step4Done { get => _step4Done; set => SetProperty(ref _step4Done, value); }

    // ================ COMMANDS ================
    public RelayCommand SelectAllCommand { get; }
    public RelayCommand DeselectAllCommand { get; }
    public RelayCommand RecheckCommand { get; }
    public RelayCommand BrowseFolderCommand { get; }
    public RelayCommand AdvancedSettingsCommand { get; }
    public RelayCommand GenerateCommand { get; }

    private async Task RunGenerateAsync()
    {
        FilesDone = 0;
        FilesTotal = Templates.Count(t => t.IsSelected);
        Step1Done = Step2Done = Step3Done = Step4Done = 0;

        // Bước 1: Kiểm tra dữ liệu
        await Task.Delay(400);
        Step1Done = 1;

        // Bước 2/3: Tạo + xuất file (giả lập)
        Step2Done = 1;
        for (int i = 0; i < FilesTotal; i++)
        {
            await Task.Delay(250);
            FilesDone = i + 1;
        }
        Step3Done = 1;

        // Bước 4: Hoàn tất
        await Task.Delay(200);
        Step4Done = 1;

        DialogHelper.Info(
            $"Đã sinh {FilesDone}/{FilesTotal} file vào:\n{OutputDir}\n" +
            $"Định dạng: {ExportFormat}\n" +
            $"Quy tắc đặt tên: {FileNamePattern}\n\n" +
            "Engine thật sẽ gọi WordMergeEngine + DynamicTableMerger ở phần sau.",
            "Sinh bộ hồ sơ");
    }
}
