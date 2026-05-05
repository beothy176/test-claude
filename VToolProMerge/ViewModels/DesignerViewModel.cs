using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using VToolProMerge.Helpers;
using VToolProMerge.Models;
using VToolProMerge.Services;

namespace VToolProMerge.ViewModels;

public class DesignerViewModel : ViewModelBase
{
    private string _templateName = "Hợp đồng mua bán";
    private bool _isDesignMode = true;
    private string _previewDataset = "Mẫu dữ liệu 01";
    private string _activeRightTab = "DataSource"; // DataSource | Element | QuickBuilder | Properties
    private double _zoom = 100;
    private string _fontName = "Times New Roman";
    private double _fontSize = 12;
    private int _pageNumber = 1;
    private int _pageCount = 4;
    private int _wordCount = 712;

    // Selected element properties (mock — bảng hàng hóa)
    private string _selectedElementType = "Table / List";
    private string _selectedElementName = "tbl_HangHoa";
    private string _selectedElementSource = "HangHoa";
    private int _previewRows = 5;
    private bool _repeatHeader = true;
    private bool _repeatWhenEmpty;
    private int _headerRepeatCount = 1;
    private string _borderStyle = "Tất cả";
    private string _cellAlign = "Giữa";
    private double _lineSpacing = 1.0;

    private readonly PreviewService? _previewService;
    private readonly DynamicTableMerger? _tables;
    private readonly ConditionalBlockProcessor? _cond;
    private readonly DataSourceManager? _sources;

    public DesignerViewModel(IEnumerable<DataSourceNode> dataSourceTree, IEnumerable<ElementNode> elements,
        PreviewService? previewService = null,
        DynamicTableMerger? tables = null,
        ConditionalBlockProcessor? cond = null,
        DataSourceManager? sources = null)
    {
        _previewService = previewService;
        _tables = tables;
        _cond = cond;
        _sources = sources;
        DataSourceTree = new ObservableCollection<DataSourceNode>(dataSourceTree);
        Elements = new ObservableCollection<ElementNode>(elements);

        QuickBuilders = new ObservableCollection<QuickBuilderItem>
        {
            new("📋", "Tạo khối thông tin nhà thầu"),
            new("🛒", "Tạo bảng hàng hóa"),
            new("✍",  "Tạo khối ký tên"),
            new("⚖", "Tạo căn cứ pháp lý"),
            new("📎", "Tạo phụ lục"),
        };

        PreviewDatasets = new ObservableCollection<string>
        {
            "Mẫu dữ liệu 01", "Mẫu dữ liệu 02", "Mẫu dữ liệu 03"
        };

        SaveCommand = new RelayCommand(_ => DoSave(askPath: false));
        SaveAsCommand = new RelayCommand(_ => DoSave(askPath: true));
        InsertFieldCommand = new RelayCommand(_ => DoInsertField());
        ScanPlaceholderCommand = new RelayCommand(_ => DoScanPlaceholder());
        InsertTableCommand = new RelayCommand(_ => DoInsertTable());
        InsertConditionCommand = new RelayCommand(_ => DoInsertCondition());
        InsertImageCommand = new RelayCommand(_ => DoInsertImage());
        InsertSubdocCommand = new RelayCommand(_ => DoInsertSubdoc());
        PreviewCommand = new RelayCommand(_ => RunPreview());
        CheckErrorsCommand = new RelayCommand(_ => DoCheckErrors());

        SwitchTabCommand = new RelayCommand(p => { if (p is string s) ActiveRightTab = s; });
        QuickBuilderCommand = new RelayCommand(p => DoQuickBuilder(p as string));
        InsertNodeCommand = new RelayCommand(p =>
        {
            if (p is DataSourceNode n) DoInsertFromNode(n);
        });
    }

    private string _lastAction = "Sẵn sàng — bấm nút trên thanh công cụ để thiết kế mẫu.";
    public string LastAction { get => _lastAction; set => SetProperty(ref _lastAction, value); }

    // ============================================================
    //                  COMMAND HANDLERS — REAL
    // ============================================================

    private void DoSave(bool askPath)
    {
        try
        {
            string targetDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VToolProMerge", "Templates");
            Directory.CreateDirectory(targetDir);

            string fileName;
            if (askPath)
            {
                var name = InputDialog.Prompt(
                    "Tên mẫu (sẽ lưu thành .json metadata + giữ tham chiếu DOCX):",
                    "Lưu với tên...", TemplateName);
                if (string.IsNullOrWhiteSpace(name)) { LastAction = "Đã huỷ lưu."; return; }
                TemplateName = name;
            }
            fileName = FileNameHelper.Sanitize(TemplateName) + ".vtm.json";
            var path = Path.Combine(targetDir, fileName);

            var meta = new
            {
                TemplateName,
                FontName,
                FontSize,
                Elements = Elements,
                DataSourceTreeNames = new List<string>(),
                SavedAt = DateTime.Now,
            };
            foreach (var n in DataSourceTree)
                meta.DataSourceTreeNames.Add(n.Name);

            var json = System.Text.Json.JsonSerializer.Serialize(meta, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                    .Create(System.Text.Unicode.UnicodeRanges.All),
            });
            File.WriteAllText(path, json);

            LastAction = $"✓ Đã lưu mẫu vào {path}";
            DialogHelper.Info($"Đã lưu mẫu '{TemplateName}'.\n\nĐường dẫn:\n{path}", "Lưu mẫu");
        }
        catch (Exception ex)
        {
            DialogHelper.Error("Lỗi lưu mẫu: " + ex.Message);
        }
    }

    private void DoInsertField()
    {
        var name = InputDialog.Prompt(
            "Tên placeholder (chỉ chữ in hoa, số và dấu gạch dưới — VD: TEN_DON_VI):",
            "Chèn field", "TEN_TRUONG_MOI");
        if (string.IsNullOrWhiteSpace(name)) return;
        name = name.Trim().ToUpperInvariant().Replace(' ', '_');
        if (!PlaceholderRegexHelper.IsValid(name))
        {
            DialogHelper.Warn("Tên placeholder không hợp lệ. Phải bắt đầu bằng chữ cái A-Z và chỉ chứa A-Z, 0-9, _.",
                "Chèn field");
            return;
        }
        var placeholder = $"[{name}]";
        Elements.Add(new ElementNode
        {
            Name = placeholder,
            Kind = ElementKind.Field,
            Icon = "🔤",
            Description = $"Field placeholder — sẽ được thay bằng giá trị {name} khi merge.",
        });
        try { System.Windows.Clipboard.SetText(placeholder); } catch { }
        LastAction = $"✓ Đã thêm field {placeholder} (đã copy vào clipboard).";
    }

    private void DoInsertFromNode(DataSourceNode node)
    {
        if (node == null) return;
        // Tạo placeholder từ tên node — sanitize
        var raw = node.Name.ToUpperInvariant();
        var ph = new System.Text.StringBuilder();
        foreach (var ch in raw)
            ph.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        var name = ph.ToString().Trim('_');
        if (string.IsNullOrEmpty(name) || !char.IsLetter(name[0])) name = "FIELD_" + name;
        var placeholder = $"[{name}]";

        Elements.Add(new ElementNode
        {
            Name = placeholder,
            Kind = ElementKind.Field,
            Icon = node.Icon,
            Description = $"Field từ nguồn '{node.Name}'.",
        });
        try { System.Windows.Clipboard.SetText(placeholder); } catch { }
        LastAction = $"✓ Đã chèn {placeholder} từ '{node.Name}' (clipboard sẵn sàng).";
    }

    private void DoScanPlaceholder()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn file Word (.docx) để quét placeholder",
            Filter = "Word document (*.docx)|*.docx|All files|*.*",
        };
        if (dlg.ShowDialog() != true) { LastAction = "Đã huỷ quét."; return; }
        try
        {
            var scanner = new TemplateScanner();
            var found = scanner.ScanDocxPlaceholders(dlg.FileName);
            if (found.Count == 0)
            {
                DialogHelper.Info("Không tìm thấy placeholder nào trong file.", "Quét placeholder");
                LastAction = "Quét xong: 0 placeholder.";
                return;
            }
            var existing = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in Elements) existing.Add(e.Name);

            int added = 0;
            foreach (var ph in found)
            {
                var key = $"[{ph}]";
                if (existing.Contains(key)) continue;
                Elements.Add(new ElementNode
                {
                    Name = key, Kind = ElementKind.Field, Icon = "🔤",
                    Description = $"Placeholder phát hiện qua quét file {Path.GetFileName(dlg.FileName)}.",
                });
                added++;
            }
            var preview = string.Join("\n  • ", found);
            DialogHelper.Info(
                $"Đã quét {found.Count} placeholder ({added} mới được thêm vào cây phần tử):\n\n  • {preview}",
                "Quét placeholder");
            LastAction = $"✓ Quét xong: {found.Count} placeholder, +{added} phần tử mới.";
        }
        catch (Exception ex)
        {
            DialogHelper.Error("Lỗi quét file: " + ex.Message);
        }
    }

    private void DoInsertTable()
    {
        var src = InputDialog.Prompt(
            "Tên nguồn dữ liệu cho bảng động (VD: HangHoa, NhaCungCap):",
            "Chèn bảng động", "HangHoa");
        if (string.IsNullOrWhiteSpace(src)) return;
        Elements.Add(new ElementNode
        {
            Name = $"tbl_{src.Trim()}",
            Kind = ElementKind.TableList,
            Icon = "📊",
            Description = $"Bảng động lặp dòng theo nguồn '{src.Trim()}'.",
        });
        LastAction = $"✓ Đã thêm bảng động tbl_{src.Trim()}.";
    }

    private void DoInsertCondition()
    {
        var expr = InputDialog.Prompt(
            "Biểu thức điều kiện (VD: HAS_VAT == 'true'):",
            "Chèn khối điều kiện", "HAS_VAT == 'true'");
        if (string.IsNullOrWhiteSpace(expr)) return;
        Elements.Add(new ElementNode
        {
            Name = $"if({expr.Trim()})",
            Kind = ElementKind.IfCondition,
            Icon = "🔀",
            Description = $"Khối hiển thị có điều kiện: {expr.Trim()}",
        });
        LastAction = $"✓ Đã thêm khối điều kiện.";
    }

    private void DoInsertImage()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn ảnh để chèn (PNG, JPG, BMP)",
            Filter = "Hình ảnh|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
        };
        if (dlg.ShowDialog() != true) return;
        Elements.Add(new ElementNode
        {
            Name = Path.GetFileName(dlg.FileName),
            Kind = ElementKind.DynamicImage,
            Icon = "🖼",
            Description = "Ảnh động — sẽ chèn từ: " + dlg.FileName,
        });
        LastAction = $"✓ Đã thêm ảnh '{Path.GetFileName(dlg.FileName)}'.";
    }

    private void DoInsertSubdoc()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn file phụ lục Word (.docx)",
            Filter = "Word document (*.docx)|*.docx|All files|*.*",
        };
        if (dlg.ShowDialog() != true) return;
        Elements.Add(new ElementNode
        {
            Name = Path.GetFileNameWithoutExtension(dlg.FileName),
            Kind = ElementKind.Subdocument,
            Icon = "📎",
            Description = "Phụ lục: " + dlg.FileName,
        });
        LastAction = $"✓ Đã thêm phụ lục '{Path.GetFileName(dlg.FileName)}'.";
    }

    private void DoCheckErrors()
    {
        var checker = new ErrorCheckerService();
        var ctx = (_sources != null && _sources.HasAny) ? _sources.BuildContext() : BuildDemoContext();

        // Lấy danh sách placeholder thật trong Elements (kiểu Field)
        var inTpl = new List<string>();
        var nameSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in Elements)
        {
            if (e.Kind != ElementKind.Field) continue;
            var raw = e.Name.Trim('[', ']');
            if (PlaceholderRegexHelper.IsValid(raw) && nameSet.Add(raw))
                inTpl.Add(raw);
        }

        var missing = new List<string>(checker.CheckMissingPlaceholders(inTpl, ctx));
        var unused  = new List<string>(checker.CheckUnusedData(inTpl, ctx));
        int tableCount = 0, condCount = 0, imageCount = 0, subdocCount = 0;
        foreach (var e in Elements)
        {
            if (e.Kind == ElementKind.TableList) tableCount++;
            else if (e.Kind == ElementKind.IfCondition) condCount++;
            else if (e.Kind == ElementKind.DynamicImage) imageCount++;
            else if (e.Kind == ElementKind.Subdocument) subdocCount++;
        }

        var msg = new System.Text.StringBuilder();
        msg.AppendLine($"== TỔNG QUAN MẪU '{TemplateName}' ==");
        msg.AppendLine($"  • {inTpl.Count} placeholder (field)");
        msg.AppendLine($"  • {tableCount} bảng động");
        msg.AppendLine($"  • {condCount} khối điều kiện");
        msg.AppendLine($"  • {imageCount} ảnh, {subdocCount} phụ lục");
        msg.AppendLine();
        if (missing.Count == 0)
            msg.AppendLine("✓ Không có placeholder thiếu dữ liệu.");
        else
        {
            msg.AppendLine($"✗ {missing.Count} placeholder THIẾU dữ liệu trong nguồn:");
            foreach (var p in missing) msg.AppendLine("    • [" + p + "]");
        }
        msg.AppendLine();
        if (unused.Count == 0)
            msg.AppendLine("✓ Mọi token nguồn đều được dùng trong mẫu.");
        else
        {
            msg.AppendLine($"⚠ {unused.Count} token có trong nguồn nhưng KHÔNG dùng trong mẫu:");
            foreach (var p in unused) msg.AppendLine("    • [" + p + "]");
        }

        bool hasError = missing.Count > 0;
        if (hasError) DialogHelper.Warn(msg.ToString(), "Kiểm tra lỗi");
        else DialogHelper.Info(msg.ToString(), "Kiểm tra lỗi");

        LastAction = hasError
            ? $"✗ Phát hiện {missing.Count} placeholder thiếu dữ liệu."
            : $"✓ Mẫu hợp lệ — {inTpl.Count} placeholder đầy đủ.";
    }

    private void DoQuickBuilder(string? title)
    {
        if (string.IsNullOrEmpty(title)) return;
        // Tạo nhanh các block hay dùng — thêm vào Elements
        var added = title switch
        {
            "Tạo khối thông tin nhà thầu" => AddBatch(new[]
            {
                ("[NT_TEN]", ElementKind.Field, "🔤", "Tên nhà thầu"),
                ("[NT_MST]", ElementKind.Field, "🔤", "Mã số thuế nhà thầu"),
                ("[NT_DIA_CHI]", ElementKind.Field, "🔤", "Địa chỉ nhà thầu"),
                ("[NT_DAI_DIEN]", ElementKind.Field, "🔤", "Người đại diện nhà thầu"),
            }),
            "Tạo bảng hàng hóa" => AddBatch(new[]
            {
                ("tbl_HangHoa", ElementKind.TableList, "📊", "Bảng động hàng hóa: STT, Tên, ĐVT, SL, Đơn giá, Thành tiền"),
            }),
            "Tạo khối ký tên" => AddBatch(new[]
            {
                ("[DONVI_NGUOI_DD]", ElementKind.Field, "🔤", "Người ký bên A"),
                ("[NT_DAI_DIEN]",    ElementKind.Field, "🔤", "Người ký bên B"),
            }),
            "Tạo căn cứ pháp lý" => AddBatch(new[]
            {
                ("[CAN_CU_LUAT]",    ElementKind.Field, "📜", "Căn cứ luật"),
                ("[CAN_CU_NGHI_DINH]", ElementKind.Field, "📜", "Căn cứ nghị định"),
            }),
            "Tạo phụ lục" => AddBatch(new[]
            {
                ("phu_luc_01", ElementKind.Subdocument, "📎", "Phụ lục đính kèm 01"),
            }),
            _ => 0,
        };
        LastAction = added > 0
            ? $"✓ Quick Builder '{title}': đã thêm {added} phần tử."
            : $"Quick Builder '{title}' chưa có template.";
    }

    private int AddBatch((string name, ElementKind kind, string icon, string desc)[] items)
    {
        int n = 0;
        var existing = new HashSet<string>();
        foreach (var e in Elements) existing.Add(e.Name);
        foreach (var (name, kind, icon, desc) in items)
        {
            if (existing.Contains(name)) continue;
            Elements.Add(new ElementNode { Name = name, Kind = kind, Icon = icon, Description = desc });
            n++;
        }
        return n;
    }

    public string TemplateName { get => _templateName; set => SetProperty(ref _templateName, value); }
    public bool IsDesignMode { get => _isDesignMode; set { SetProperty(ref _isDesignMode, value); OnPropertyChanged(nameof(IsEndUserMode)); } }
    public bool IsEndUserMode => !_isDesignMode;
    public string PreviewDataset { get => _previewDataset; set => SetProperty(ref _previewDataset, value); }
    public string ActiveRightTab { get => _activeRightTab; set => SetProperty(ref _activeRightTab, value); }
    public double Zoom { get => _zoom; set => SetProperty(ref _zoom, value); }
    public string FontName { get => _fontName; set => SetProperty(ref _fontName, value); }
    public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
    public int PageNumber { get => _pageNumber; set => SetProperty(ref _pageNumber, value); }
    public int PageCount { get => _pageCount; set => SetProperty(ref _pageCount, value); }
    public int WordCount { get => _wordCount; set => SetProperty(ref _wordCount, value); }

    public ObservableCollection<DataSourceNode> DataSourceTree { get; }
    public ObservableCollection<ElementNode> Elements { get; }
    public ObservableCollection<QuickBuilderItem> QuickBuilders { get; }
    public ObservableCollection<string> PreviewDatasets { get; }

    public string SelectedElementType { get => _selectedElementType; set => SetProperty(ref _selectedElementType, value); }
    public string SelectedElementName { get => _selectedElementName; set => SetProperty(ref _selectedElementName, value); }
    public string SelectedElementSource { get => _selectedElementSource; set => SetProperty(ref _selectedElementSource, value); }
    public int PreviewRows { get => _previewRows; set => SetProperty(ref _previewRows, value); }
    public bool RepeatHeader { get => _repeatHeader; set => SetProperty(ref _repeatHeader, value); }
    public bool RepeatWhenEmpty { get => _repeatWhenEmpty; set => SetProperty(ref _repeatWhenEmpty, value); }
    public int HeaderRepeatCount { get => _headerRepeatCount; set => SetProperty(ref _headerRepeatCount, value); }
    public string BorderStyle { get => _borderStyle; set => SetProperty(ref _borderStyle, value); }
    public string CellAlign { get => _cellAlign; set => SetProperty(ref _cellAlign, value); }
    public double LineSpacing { get => _lineSpacing; set => SetProperty(ref _lineSpacing, value); }

    public RelayCommand SaveCommand { get; }
    public RelayCommand SaveAsCommand { get; }
    public RelayCommand InsertFieldCommand { get; }
    public RelayCommand ScanPlaceholderCommand { get; }
    public RelayCommand InsertTableCommand { get; }
    public RelayCommand InsertConditionCommand { get; }
    public RelayCommand InsertImageCommand { get; }
    public RelayCommand InsertSubdocCommand { get; }
    public RelayCommand PreviewCommand { get; }
    public RelayCommand CheckErrorsCommand { get; }
    public RelayCommand SwitchTabCommand { get; }
    public RelayCommand QuickBuilderCommand { get; }
    public RelayCommand InsertNodeCommand { get; }

    public class QuickBuilderItem
    {
        public QuickBuilderItem(string icon, string title) { Icon = icon; Title = title; }
        public string Icon { get; }
        public string Title { get; }
    }

    private void RunPreview()
    {
        if (_previewService == null)
        {
            DialogHelper.Info("Preview chưa được cấu hình.", "Xem thử");
            return;
        }
        try
        {
            // Tạo template demo trong %TEMP% rồi merge với context (ưu tiên data source user đã tải).
            var dir = Path.Combine(Path.GetTempPath(), "VToolPro_DesignerPreview");
            Directory.CreateDirectory(dir);
            var templatePath = SampleTemplateBuilder.CreateContractTemplate(dir);

            MergeContext ctx;
            if (_sources != null && _sources.HasAny) ctx = _sources.BuildContext();
            else ctx = BuildDemoContext();

            var preview = _previewService.CreatePreviewDocument(templatePath, ctx, _cond, _tables);
            _previewService.OpenInWord(preview);
        }
        catch (Exception ex)
        {
            DialogHelper.Error("Lỗi preview: " + ex.Message);
        }
    }

    private static MergeContext BuildDemoContext()
    {
        var ctx = new MergeContext();
        ctx.Tokens["[DONVI_TEN]"] = "CÔNG TY TNHH ABC";
        ctx.Tokens["[DONVI_MST]"] = "0312345678";
        ctx.Tokens["[DONVI_DIA_CHI]"] = "Số 1 Nguyễn Văn Cừ, Q.1, TP.HCM";
        ctx.Tokens["[DONVI_NGUOI_DD]"] = "Nguyễn Văn An";
        ctx.Tokens["[NT_TEN]"] = "CÔNG TY CP XYZ";
        ctx.Tokens["[NT_MST]"] = "0398765432";
        ctx.Tokens["[NT_DIA_CHI]"] = "10 Trần Hưng Đạo, Hà Nội";
        ctx.Tokens["[NT_DAI_DIEN]"] = "Trần Thị Bích";
        ctx.Tokens["[SO_HOP_DONG]"] = "DEMO/2025/HD-MB";
        ctx.Tokens["[NGAY_KY]"] = DateTime.Now.ToString("dd/MM/yyyy");
        ctx.Tokens["[TONG_TIEN]"] = "100.000.000";
        ctx.Tokens["[HAS_VAT]"] = "true";

        var hh = new System.Data.DataTable("HangHoa");
        hh.Columns.Add("STT");
        hh.Columns.Add("MA");
        hh.Columns.Add("TEN");
        hh.Columns.Add("DVT");
        hh.Columns.Add("SL");
        hh.Columns.Add("DG");
        hh.Columns.Add("TT");
        hh.Rows.Add(1, "M001", "Sản phẩm A", "Cái", "10", "1.000.000", "10.000.000");
        hh.Rows.Add(2, "M002", "Sản phẩm B", "Bộ", "5", "2.000.000", "10.000.000");
        ctx.TableSources["HH"] = hh;
        return ctx;
    }
}
