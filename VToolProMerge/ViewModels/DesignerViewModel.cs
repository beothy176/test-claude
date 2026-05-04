using System.Collections.Generic;
using System.Collections.ObjectModel;
using VToolProMerge.Helpers;
using VToolProMerge.Models;

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

    public DesignerViewModel(IEnumerable<DataSourceNode> dataSourceTree, IEnumerable<ElementNode> elements)
    {
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

        SaveCommand = new RelayCommand(_ => DialogHelper.Info("Đã lưu mẫu (khung).", "Lưu"));
        SaveAsCommand = new RelayCommand(_ => DialogHelper.Info("Lưu với tên...", "Lưu"));
        InsertFieldCommand = new RelayCommand(_ => DialogHelper.Info(
            "Mở Picker chọn placeholder để chèn vào trang Word.", "Chèn field"));
        ScanPlaceholderCommand = new RelayCommand(_ => DialogHelper.Info(
            "TemplateScanner sẽ quét toàn bộ placeholder dạng [TEN_TRUONG] trong tài liệu.",
            "Quét placeholder"));
        InsertTableCommand = new RelayCommand(_ => DialogHelper.Info(
            "Chèn bảng động lặp dòng (DynamicTable).", "Bảng động"));
        InsertConditionCommand = new RelayCommand(_ => DialogHelper.Info(
            "Chèn khối điều kiện If/Else.", "Điều kiện"));
        InsertImageCommand = new RelayCommand(_ => DialogHelper.Info(
            "Chèn ảnh động (Dynamic Image).", "Chèn ảnh"));
        InsertSubdocCommand = new RelayCommand(_ => DialogHelper.Info(
            "Chèn phụ lục dưới dạng Subdocument.", "Chèn phụ lục"));
        PreviewCommand = new RelayCommand(_ => DialogHelper.Info(
            "Mở preview bằng Microsoft Word (Interop).", "Xem thử"));
        CheckErrorsCommand = new RelayCommand(_ => DialogHelper.Info(
            "Chạy ErrorCheckerService cho mẫu hiện tại.", "Kiểm tra lỗi"));

        SwitchTabCommand = new RelayCommand(p => { if (p is string s) ActiveRightTab = s; });
        QuickBuilderCommand = new RelayCommand(p => DialogHelper.Info(
            $"Quick Builder: {p}", "Quick Builder"));
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

    public class QuickBuilderItem
    {
        public QuickBuilderItem(string icon, string title) { Icon = icon; Title = title; }
        public string Icon { get; }
        public string Title { get; }
    }
}
