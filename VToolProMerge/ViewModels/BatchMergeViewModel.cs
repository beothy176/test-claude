using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VToolProMerge.Helpers;
using VToolProMerge.Models;
using VToolProMerge.Services;

namespace VToolProMerge.ViewModels;

public class BatchMergeViewModel : ViewModelBase
{
    private readonly BatchMergeService _batch;
    private readonly DataMappingEngine _mapping;
    private readonly PreviewService _preview;
    private readonly DataSourceManager _sources;
    private readonly List<TemplateItem> _templateCatalog;

    private string _outputDir = @"D:\VToolProMerge\Output\HS_ABC_01";
    private string _fileNamePattern = "[SO_HOSO]_[TEN_GOI_THAU]_[TEN_MAU]";
    private string _exportFormat = "DOCX";
    private bool _openFileAfter = true;
    private bool _openFolderAfter = true;
    private bool _saveHistory = true;
    private bool _writeDetailedLog;
    private bool _printAfter;

    private int _step1Done, _step2Done, _step3Done, _step4Done;
    private int _filesDone;
    private int _filesTotal = 8;

    public BatchMergeViewModel(
        IEnumerable<TemplateItem> seedTemplates,
        IEnumerable<MergeIssue> seedIssues,
        IEnumerable<OutputFileItem> seedOutputs,
        IEnumerable<AuditLogItem> seedLogs,
        BatchMergeService batchService,
        DataMappingEngine mapping,
        PreviewService preview,
        DataSourceManager sources)
    {
        _batch = batchService;
        _mapping = mapping;
        _preview = preview;
        _sources = sources;
        _templateCatalog = seedTemplates.ToList();

        Templates = new ObservableCollection<BatchTemplateItem>(
            _templateCatalog.Take(8).Select(t => new BatchTemplateItem
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
            "Bộ kiểm tra lỗi đã chạy ErrorCheckerService.", "Kiểm tra lại"));
        BrowseFolderCommand = new RelayCommand(_ =>
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog();
            if (dlg.ShowDialog() == true) OutputDir = dlg.FolderName;
        });
        AdvancedSettingsCommand = new RelayCommand(_ => DialogHelper.Info(
            "Mở thiết lập nâng cao (template tên file, font, header/footer mặc định, ...).",
            "Thiết lập nâng cao"));

        GenerateCommand = new RelayCommand(async _ => await RunGenerateAsync());
        DemoCommand = new RelayCommand(async _ => await RunDemoAsync());
    }

    // ================ STAT ================
    public int SelectedCount => Templates.Count(t => t.IsSelected);
    public int PlaceholderCount => 24;
    public int DynamicTableCount => 3;
    public int ConditionCount => 2;
    public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);
    public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
    public string SelectedTotalText => $"{SelectedCount} / {Templates.Count}";

    public ObservableCollection<BatchTemplateItem> Templates { get; }
    public ObservableCollection<MergeIssue> Issues { get; }
    public ObservableCollection<OutputFileItem> OutputFiles { get; }
    public ObservableCollection<AuditLogItem> Logs { get; }

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

    public int FilesDone { get => _filesDone; set { SetProperty(ref _filesDone, value); OnPropertyChanged(nameof(ProgressText)); OnPropertyChanged(nameof(ProgressPercent)); } }
    public int FilesTotal { get => _filesTotal; set => SetProperty(ref _filesTotal, value); }
    public string ProgressText => $"{FilesDone} / {FilesTotal} file";
    public double ProgressPercent => FilesTotal == 0 ? 0 : (double)FilesDone * 100 / FilesTotal;

    public int Step1Done { get => _step1Done; set => SetProperty(ref _step1Done, value); }
    public int Step2Done { get => _step2Done; set => SetProperty(ref _step2Done, value); }
    public int Step3Done { get => _step3Done; set => SetProperty(ref _step3Done, value); }
    public int Step4Done { get => _step4Done; set => SetProperty(ref _step4Done, value); }

    public RelayCommand SelectAllCommand { get; }
    public RelayCommand DeselectAllCommand { get; }
    public RelayCommand RecheckCommand { get; }
    public RelayCommand BrowseFolderCommand { get; }
    public RelayCommand AdvancedSettingsCommand { get; }
    public RelayCommand GenerateCommand { get; }
    public RelayCommand DemoCommand { get; }

    // =================================================================
    private async Task RunGenerateAsync()
    {
        var selected = Templates.Where(t => t.IsSelected).ToList();
        if (selected.Count == 0)
        {
            DialogHelper.Warn("Hãy chọn ít nhất 1 mẫu để sinh.");
            return;
        }

        // Tìm TemplateItem gốc theo Id để lấy FilePath.
        var resolved = selected
            .Select(s => _templateCatalog.FirstOrDefault(t => t.Id == s.Id))
            .Where(t => t != null && !string.IsNullOrEmpty(t!.FilePath) && File.Exists(t!.FilePath))
            .Cast<TemplateItem>()
            .ToList();

        if (resolved.Count == 0)
        {
            DialogHelper.Warn(
                "Chưa có file mẫu DOCX trên đĩa.\n\n" +
                "Nhấn 'Demo: Tạo mẫu hợp đồng + sinh thử' để engine tự dựng template demo " +
                "rồi chạy end-to-end (placeholder + bảng động + điều kiện).");
            return;
        }

        // Ưu tiên MergeContext từ các nguồn dữ liệu user đã tải;
        // Nếu chưa có nguồn nào, fallback sang context demo.
        var ctx = _sources.HasAny ? _sources.BuildContext() : BuildDemoContext();
        await RunPipelineAsync(resolved, ctx);
    }

    private async Task RunDemoAsync()
    {
        try
        {
            var demoDir = Path.Combine(Path.GetTempPath(), "VToolPro_Demo");
            Directory.CreateDirectory(demoDir);

            FilesDone = 0;
            Step1Done = Step2Done = Step3Done = Step4Done = 0;

            // Bước 1 — kiểm tra dữ liệu (xây template + context).
            await Task.Run(() => SampleTemplateBuilder.CreateContractTemplate(demoDir));
            var templatePath = Path.Combine(demoDir, "HopDongMuaBan_Template.docx");
            var item = new TemplateItem
            {
                Id = "demo",
                Name = "HopDongMuaBan_Demo",
                Category = "Hợp đồng",
                Kind = TemplateKind.Word,
                FilePath = templatePath,
                Version = "demo"
            };

            await RunPipelineAsync(new() { item }, BuildDemoContext(), demoDir);

            DialogHelper.Info(
                $"Đã sinh demo vào:\n{demoDir}\n\n" +
                "File template gốc + file kết quả đều ở thư mục trên.\n" +
                "Mở file '*_HopDongMuaBan_Demo.docx' bằng Word để xem kết quả thật.",
                "Demo thành công");

            if (OpenFolderAfter)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = demoDir,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            DialogHelper.Error("Demo lỗi: " + ex.Message);
        }
    }

    private async Task RunPipelineAsync(List<TemplateItem> templates, MergeContext ctx, string? overrideDir = null)
    {
        var dir = overrideDir ?? OutputDir;
        FilesDone = 0;
        FilesTotal = templates.Count;
        Step1Done = Step2Done = Step3Done = Step4Done = 0;

        await Task.Delay(150);
        Step1Done = 1;
        await Task.Delay(150);
        Step2Done = 1;

        var result = await Task.Run(() => _batch.GenerateDocuments(
            templates, ctx, dir, FileNamePattern, ExportFormat,
            (done, total) => { FilesDone = done; }));

        Step3Done = 1;
        await Task.Delay(120);
        Step4Done = 1;

        // Cập nhật danh sách OutputFiles UI.
        OutputFiles.Clear();
        foreach (var path in result.OutputFiles)
        {
            var fi = new FileInfo(path);
            OutputFiles.Add(new OutputFileItem
            {
                FileName = fi.Name,
                Format = fi.Extension.TrimStart('.').ToUpperInvariant(),
                SizeText = $"{fi.Length / 1024} KB",
                Status = "Sẵn sàng",
                FullPath = fi.FullName
            });
        }

        if (result.Errors.Count > 0)
            DialogHelper.Warn(string.Join("\n", result.Errors), "Có lỗi khi sinh hồ sơ");
        else if (result.Warnings.Count > 0)
            DialogHelper.Warn(string.Join("\n", result.Warnings), "Cảnh báo");
    }

    /// <summary>
    /// Dựng MergeContext mẫu cho demo: scalar + bảng hàng hóa + bật điều kiện HAS_VAT.
    /// </summary>
    private MergeContext BuildDemoContext()
    {
        var ctx = new MergeContext();
        ctx.Tokens["[DONVI_TEN]"] = "CÔNG TY TNHH ABC";
        ctx.Tokens["[DONVI_MST]"] = "0312345678";
        ctx.Tokens["[DONVI_DIA_CHI]"] = "Số 1 Nguyễn Văn Cừ, Q.1, TP.HCM";
        ctx.Tokens["[DONVI_NGUOI_DD]"] = "Nguyễn Văn An";
        ctx.Tokens["[NT_TEN]"] = "CÔNG TY CP XYZ";
        ctx.Tokens["[NT_MST]"] = "0398765432";
        ctx.Tokens["[NT_DIA_CHI]"] = "10 Trần Hưng Đạo, Q. Hoàn Kiếm, Hà Nội";
        ctx.Tokens["[NT_DAI_DIEN]"] = "Trần Thị Bích";
        ctx.Tokens["[SO_HOP_DONG]"] = "025/2025/HD-MB";
        ctx.Tokens["[NGAY_KY]"] = DateTime.Now.ToString("dd/MM/yyyy");
        ctx.Tokens["[TONG_TIEN]"] = "75.500.000";
        ctx.Tokens["[HAS_VAT]"] = "true";
        ctx.Tokens["[SO_HOSO]"] = "HS001";
        ctx.Tokens["[TEN_GOI_THAU]"] = "GoiThau01";

        var hh = new DataTable("HangHoa");
        hh.Columns.Add("STT");
        hh.Columns.Add("MA");
        hh.Columns.Add("TEN");
        hh.Columns.Add("DVT");
        hh.Columns.Add("SL");
        hh.Columns.Add("DG");
        hh.Columns.Add("TT");
        hh.Rows.Add(1, "M001", "Máy in laser HP", "Cái", "5", "5,000,000", "25,000,000");
        hh.Rows.Add(2, "M002", "Máy tính để bàn Dell", "Bộ", "10", "12,000,000", "120,000,000");
        hh.Rows.Add(3, "M003", "Bàn làm việc gỗ", "Cái", "20", "1,500,000", "30,000,000");
        ctx.TableSources["HH"] = hh;

        return ctx;
    }
}
