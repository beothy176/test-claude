using System;
using System.Collections.Generic;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

/// <summary>
/// Cung cấp dữ liệu mẫu ban đầu để Demo UI chạy được mà chưa cần Word/Excel thật.
/// Sau này thay bằng JsonTemplateRepository / SqliteRepository.
/// </summary>
public class SeedData
{
    public List<TemplateItem> Templates { get; } = new();
    public List<PlaceholderItem> Placeholders { get; } = new();
    public List<MergeIssue> Issues { get; } = new();
    public List<OutputFileItem> OutputFiles { get; } = new();
    public List<AuditLogItem> Logs { get; } = new();
    public List<DataSourceNode> DataSourceTree { get; } = new();
    public List<ElementNode> Elements { get; } = new();

    public static SeedData Build()
    {
        var s = new SeedData();
        s.BuildTemplates();
        s.BuildPlaceholders();
        s.BuildIssues();
        s.BuildOutputs();
        s.BuildLogs();
        s.BuildDataSourceTree();
        s.BuildElements();
        return s;
    }

    private void BuildTemplates()
    {
        var d = new DateTime(2024, 5, 15, 14, 30, 0);
        Templates.Add(new TemplateItem { Name = "Báo giá tiêu chuẩn",   Category = "Báo giá",   Description = "Mẫu báo giá tiêu chuẩn cho khách hàng doanh nghiệp.", Version = "v1.2.0", UpdatedAt = d, Author = "Nguyễn Văn An",   Kind = TemplateKind.Word,  SizeBytes = 78 * 1024,  PlaceholderCount = 28, DynamicTableCount = 3, ConditionCount = 5, UseCount = 23,  LastUsedAt = new DateTime(2024,5,16,9,12,0)});
        Templates.Add(new TemplateItem { Name = "Hợp đồng mua bán",     Category = "Hợp đồng",  Description = "Mẫu hợp đồng mua bán hàng hóa thông dụng.",            Version = "v1.3.0", UpdatedAt = new DateTime(2024,5,2),  Author = "Nguyễn Văn An",   Kind = TemplateKind.Word,  SizeBytes = 92 * 1024,  PlaceholderCount = 35, DynamicTableCount = 2, ConditionCount = 7});
        Templates.Add(new TemplateItem { Name = "Hồ sơ mời thầu",       Category = "Đấu thầu",  Description = "Mẫu hồ sơ mời thầu theo quy định hiện hành.",          Version = "v1.1.0", UpdatedAt = new DateTime(2024,4,25), Author = "Trần Thị Bích",   Kind = TemplateKind.Word,  SizeBytes = 156 * 1024, PlaceholderCount = 64, DynamicTableCount = 6, ConditionCount = 9});
        Templates.Add(new TemplateItem { Name = "Bảng chào giá chi tiết", Category = "Báo giá", Description = "Bảng chào giá chi tiết theo hạng mục và vật tư.",     Version = "v1.0.0", UpdatedAt = new DateTime(2024,5,10), Author = "Lê Minh Đức",     Kind = TemplateKind.Excel, SizeBytes = 42 * 1024,  PlaceholderCount = 12, DynamicTableCount = 1, ConditionCount = 0});
        Templates.Add(new TemplateItem { Name = "Quyết định phê duyệt", Category = "Quyết định", Description = "Mẫu quyết định phê duyệt dự án hoặc nhiệm vụ.",       Version = "v1.1.0", UpdatedAt = new DateTime(2024,4,18), Author = "Nguyễn Văn An",   Kind = TemplateKind.Word,  SizeBytes = 48 * 1024,  PlaceholderCount = 18, DynamicTableCount = 0, ConditionCount = 3});
        Templates.Add(new TemplateItem { Name = "Báo cáo tổng hợp",     Category = "Báo cáo",   Description = "Mẫu báo cáo tổng hợp kết quả thực hiện.",              Version = "v1.0.0", UpdatedAt = new DateTime(2024,5,5),  Author = "Trần Thị Bích",   Kind = TemplateKind.Word,  SizeBytes = 64 * 1024,  PlaceholderCount = 22, DynamicTableCount = 2, ConditionCount = 4});
        Templates.Add(new TemplateItem { Name = "Tờ trình đề nghị",     Category = "Tờ trình",  Description = "Mẫu tờ trình đề nghị phê duyệt hoặc chấp thuận.",      Version = "v1.0.0", UpdatedAt = new DateTime(2024,4,20), Author = "Phạm Hoàng Nam",  Kind = TemplateKind.Word,  SizeBytes = 36 * 1024,  PlaceholderCount = 16, DynamicTableCount = 0, ConditionCount = 2});
        Templates.Add(new TemplateItem { Name = "Danh sách nhà cung cấp", Category = "Danh mục", Description = "Danh sách và thông tin nhà cung cấp, đối tác.",       Version = "v1.2.0", UpdatedAt = new DateTime(2024,5,12), Author = "Lê Minh Đức",     Kind = TemplateKind.Excel, SizeBytes = 58 * 1024,  PlaceholderCount = 8,  DynamicTableCount = 1, ConditionCount = 0});
        Templates.Add(new TemplateItem { Name = "Phụ lục hợp đồng",     Category = "Hợp đồng",  Description = "Mẫu phụ lục hợp đồng dùng chung cho các hợp đồng.",   Version = "v1.0.0", UpdatedAt = new DateTime(2024,5,8),  Author = "Nguyễn Văn An",   Kind = TemplateKind.Word,  SizeBytes = 28 * 1024,  PlaceholderCount = 14, DynamicTableCount = 1, ConditionCount = 2});
    }

    private void BuildPlaceholders()
    {
        // Đơn vị
        Placeholders.Add(new() { FieldName = "DONVI_TEN",      DisplayName = "Tên đơn vị",        Group = "Thông tin đơn vị", Required = true });
        Placeholders.Add(new() { FieldName = "DONVI_MST",      DisplayName = "Mã số thuế đơn vị", Group = "Thông tin đơn vị" });
        Placeholders.Add(new() { FieldName = "DONVI_DIA_CHI",  DisplayName = "Địa chỉ đơn vị",    Group = "Thông tin đơn vị" });
        Placeholders.Add(new() { FieldName = "DONVI_NGUOI_DD", DisplayName = "Người đại diện",    Group = "Thông tin đơn vị" });
        Placeholders.Add(new() { FieldName = "DONVI_CHUC_VU",  DisplayName = "Chức vụ",           Group = "Thông tin đơn vị" });

        // Nhà thầu
        Placeholders.Add(new() { FieldName = "NT_TEN",         DisplayName = "Tên nhà thầu",      Group = "Thông tin nhà thầu", Required = true });
        Placeholders.Add(new() { FieldName = "NT_MST",         DisplayName = "Mã số thuế",        Group = "Thông tin nhà thầu" });
        Placeholders.Add(new() { FieldName = "NT_DIA_CHI",     DisplayName = "Địa chỉ",           Group = "Thông tin nhà thầu" });
        Placeholders.Add(new() { FieldName = "NT_DAI_DIEN",    DisplayName = "Đại diện",          Group = "Thông tin nhà thầu" });
        Placeholders.Add(new() { FieldName = "NT_CHUC_VU",     DisplayName = "Chức vụ",           Group = "Thông tin nhà thầu" });

        // Hợp đồng
        Placeholders.Add(new() { FieldName = "SO_HOP_DONG",    DisplayName = "Số hợp đồng",       Group = "Thông tin hợp đồng", Required = true });
        Placeholders.Add(new() { FieldName = "NGAY_KY",        DisplayName = "Ngày ký",           Group = "Thông tin hợp đồng", DataType = "Date" });
        Placeholders.Add(new() { FieldName = "TONG_TIEN",      DisplayName = "Tổng tiền",         Group = "Thông tin hợp đồng", DataType = "Money" });

        // Hàng hóa
        Placeholders.Add(new() { FieldName = "HH_STT",  DisplayName = "STT",         Group = "Bảng hàng hóa", Source = "HangHoa" });
        Placeholders.Add(new() { FieldName = "HH_MA",   DisplayName = "Mã hàng",     Group = "Bảng hàng hóa", Source = "HangHoa" });
        Placeholders.Add(new() { FieldName = "HH_TEN",  DisplayName = "Tên hàng",    Group = "Bảng hàng hóa", Source = "HangHoa" });
        Placeholders.Add(new() { FieldName = "HH_DVT",  DisplayName = "ĐVT",         Group = "Bảng hàng hóa", Source = "HangHoa" });
        Placeholders.Add(new() { FieldName = "HH_SL",   DisplayName = "Số lượng",    Group = "Bảng hàng hóa", Source = "HangHoa", DataType = "Number" });
        Placeholders.Add(new() { FieldName = "HH_DG",   DisplayName = "Đơn giá",     Group = "Bảng hàng hóa", Source = "HangHoa", DataType = "Money" });
        Placeholders.Add(new() { FieldName = "HH_TT",   DisplayName = "Thành tiền",  Group = "Bảng hàng hóa", Source = "HangHoa", DataType = "Money" });
    }

    private void BuildIssues()
    {
        Issues.Add(new() { Kind = IssueKind.MissingPlaceholder,         Severity = IssueSeverity.Error,   Title = "Thiếu placeholder",        Description = "Còn 3 placeholder chưa được gắn dữ liệu.",  Count = 3, ActionLabel = "Gắn dữ liệu" });
        Issues.Add(new() { Kind = IssueKind.UnmappedData,               Severity = IssueSeverity.Warning, Title = "Dữ liệu chưa map",         Description = "Còn 1 placeholder chưa map với nguồn dữ liệu.", Count = 1, ActionLabel = "Gắn dữ liệu" });
        Issues.Add(new() { Kind = IssueKind.DynamicTableMissingColumn,  Severity = IssueSeverity.Error,   Title = "Bảng động thiếu cột",      Description = "Bảng \"Bảng chi tiết vật tư\" thiếu 1 cột dữ liệu.", Count = 1, ActionLabel = "Sửa bảng" });
        Issues.Add(new() { Kind = IssueKind.MissingTemplateFile,        Severity = IssueSeverity.Info,    Title = "File mẫu thiếu",           Description = "0 file mẫu bị thiếu.",                       Count = 0, ActionLabel = "Chi tiết" });
        Issues.Add(new() { Kind = IssueKind.MissingExcelSheet,          Severity = IssueSeverity.Info,    Title = "File Excel thiếu sheet",   Description = "0 file Excel thiếu sheet dữ liệu.",          Count = 0, ActionLabel = "Sửa nguồn" });
    }

    private void BuildOutputs()
    {
        OutputFiles.Add(new() { FileName = "01_ToTrinhDeNghi.docx",     Format = "DOCX", SizeText = "124 KB", Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "02_QuyetDinhPheDuyet.docx", Format = "DOCX", SizeText = "98 KB",  Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "03_BaoCaoThamDinh.docx",    Format = "DOCX", SizeText = "156 KB", Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "04_ThongBaoKetQua.docx",    Format = "DOCX", SizeText = "72 KB",  Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "05_HopDongMuaBan.docx",     Format = "DOCX", SizeText = "210 KB", Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "06_PhuLucHopDong.docx",     Format = "DOCX", SizeText = "88 KB",  Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "07_BienBanNghiemThu.docx",  Format = "DOCX", SizeText = "115 KB", Status = "Sẵn sàng" });
        OutputFiles.Add(new() { FileName = "08_BaoGia.docx",            Format = "DOCX", SizeText = "67 KB",  Status = "Cảnh báo" });
    }

    private void BuildLogs()
    {
        var t = new DateTime(2024, 5, 15, 14, 30, 0);
        Logs.Add(new() { Time = t,                Action = "Kiểm tra dữ liệu", Status = "Hoàn tất" });
        Logs.Add(new() { Time = t.AddMinutes(1),  Action = "Tạo file",         Status = "Chờ thực hiện" });
        Logs.Add(new() { Time = t.AddMinutes(1),  Action = "Xuất file",        Status = "Chờ thực hiện" });
        Logs.Add(new() { Time = t.AddMinutes(1),  Action = "Hoàn tất",         Status = "Chờ thực hiện" });
    }

    private void BuildDataSourceTree()
    {
        DataSourceTree.Add(new DataSourceNode { Name = "Dữ liệu chung",       Icon = "🗄" });
        DataSourceTree.Add(new DataSourceNode { Name = "Thông tin đơn vị",    Icon = "🏢" });
        DataSourceTree.Add(new DataSourceNode { Name = "Thông tin nhà thầu", Icon = "👤" });
        DataSourceTree.Add(new DataSourceNode { Name = "Danh mục dùng chung", Icon = "📂" });
        DataSourceTree.Add(new DataSourceNode { Name = "Dữ liệu Excel",       Icon = "📊" });
        DataSourceTree.Add(new DataSourceNode { Name = "Bảng động",           Icon = "📋" });
        DataSourceTree.Add(new DataSourceNode { Name = "Hồ sơ hiện tại",      Icon = "📁" });
    }

    private void BuildElements()
    {
        Elements.Add(new() { Name = "Field",         Kind = ElementKind.Field,         Icon = "🔤" });
        Elements.Add(new() { Name = "Table / List",  Kind = ElementKind.TableList,     Icon = "📋" });
        Elements.Add(new() { Name = "If Condition",  Kind = ElementKind.IfCondition,   Icon = "❓" });
        Elements.Add(new() { Name = "Header / Footer", Kind = ElementKind.HeaderFooter, Icon = "📑" });
        Elements.Add(new() { Name = "Dynamic Image", Kind = ElementKind.DynamicImage,  Icon = "🖼" });
        Elements.Add(new() { Name = "Subdocument",   Kind = ElementKind.Subdocument,   Icon = "📎" });
    }
}
