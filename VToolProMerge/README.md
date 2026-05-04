# VToolPro Merge — Phần 1 + Phần 2 (UI + Engine thật)

Phần mềm trộn thư / sinh hồ sơ chuyên nghiệp, chạy local/offline trên Windows.

## Part 1 — UI khung
MainWindow + Sidebar + Topbar + Statusbar và 3 màn hình chính theo bố cục 3 ảnh:

1. **Thư viện mẫu** — card mẫu, filter, panel "Thông tin mẫu" + "Tóm tắt placeholder".
2. **Trộn bộ hồ sơ** — 5 thẻ thống kê + 4 cột + tiến trình 4 bước + nút "Sinh bộ hồ sơ" / "Demo".
3. **Thiết kế mẫu (Designer)** — breadcrumb, toolbar, format bar, trang Word A4 mô phỏng + 4 panel phải.

## Part 2 — Engine thật (đã hoàn thành)

| Module                       | Trạng thái | Chi tiết                                                                             |
|------------------------------|------------|--------------------------------------------------------------------------------------|
| `WordMergeEngine`            | ✅          | Replace placeholder split-run-safe, áp dụng cho body/header/footer/footnote/endnote.|
| `ConditionalBlockProcessor`  | ✅          | `[IF:FIELD]…[ENDIF:FIELD]`, `[IF:FIELD=VALUE]`, `[IF:FIELD!=VALUE]`, hỗ trợ lồng.   |
| `DynamicTableMerger`         | ✅          | Tự nhận diện template row qua `[PREFIX_*]`, clone/insert per data row, format VND.  |
| `JsonDataSource`             | ✅          | Đọc `Scalars` + `Tables`.                                                            |
| `ExcelDataSource`            | ✅          | Sheet `Scalars` (key/value) + các sheet còn lại = bảng dữ liệu.                     |
| `SqliteDataSource`           | ✅          | `SELECT Key,Value FROM Scalars` + `SELECT * FROM <table>`.                           |
| `DataMappingEngine`          | ✅          | Gộp nhiều IDataSource → `MergeContext` (token + table sources).                      |
| `BatchMergeService`          | ✅          | End-to-end: condition → table → placeholder → PDF.                                   |
| `PreviewService`             | ✅          | Mở Word qua COM late-bound (fallback shell nếu Word chưa cài).                       |
| `WordMergeEngine.ExportToPdf`| ✅          | Word Interop late-bound (`ExportAsFixedFormat` → PDF).                              |
| `SampleTemplateBuilder`      | ✅          | Tự dựng 1 mẫu hợp đồng `.docx` để chạy demo end-to-end không cần Word.              |
| `ErrorCheckerService`        | ✅          | Thiếu placeholder / dữ liệu chưa map / sheet thiếu / cột bảng động thiếu / file thiếu.|

### Quy ước placeholder Part 2

- Scalar: `[TEN_TRUONG]` — thay theo `MergeContext.Tokens`.
- Bảng động: dòng template chứa `[PREFIX_FIELD]` ví dụ `[HH_STT]`, `[HH_TEN]`, `[HH_TT]`. Engine tự tìm dòng template, clone theo từng row trong `MergeContext.TableSources["HH"]`, sau đó xóa dòng template gốc.
- Khối điều kiện: paragraph chứa duy nhất `[IF:FIELD]` mở đầu, paragraph khác chứa `[ENDIF:FIELD]` đóng. Hỗ trợ `[IF:FIELD=VALUE]` và lồng nhau.
- Dòng tổng cộng (`[TONG_TIEN]`) là placeholder thường, để ngoài bảng động.

---

## 1. Mở bằng Visual Studio 2022

1. Mở Visual Studio 2022 (yêu cầu đã cài workload **.NET desktop development**).
2. `File → Open → Project/Solution → VToolProMerge.sln`.
3. Đặt project `VToolProMerge` làm Startup Project.

## 2. Cài NuGet (Visual Studio sẽ tự restore)

Project đã khai báo các package sau trong `VToolProMerge.csproj`:

| Package                       | Vai trò                                                         |
|-------------------------------|-----------------------------------------------------------------|
| `CommunityToolkit.Mvvm`       | Hỗ trợ MVVM (mở rộng phần engine)                               |
| `Microsoft.Data.Sqlite`       | Lưu lịch sử / placeholder vào SQLite                            |
| `ClosedXML`                   | Đọc/ghi Excel (`.xlsx`) cho bảng động và danh mục               |
| `DocumentFormat.OpenXml`      | Quét và thay placeholder trong DOCX không cần Word              |

Khi cần preview/xuất PDF bằng Word thật, thêm tham chiếu COM
`Microsoft.Office.Interop.Word` ở máy đã cài Microsoft Office.

## 3. Build & Run

- Visual Studio: `Ctrl+Shift+B` → `F5`.
- Hoặc dòng lệnh:

```bash
dotnet restore
dotnet build
dotnet run --project VToolProMerge
```

Yêu cầu: **.NET 8 SDK** trên Windows.

## 4. Cấu trúc dự án

```
VToolProMerge/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs   # Sidebar + Topbar + Statusbar + ContentControl
├── AssemblyInfo.cs
│
├── Models/                # TemplateItem, PlaceholderItem, MergeIssue, OutputFileItem, ...
├── ViewModels/            # MainViewModel + 9 ViewModel cho 9 màn / panel
├── Views/                 # 6 UserControl: TemplateLibraryView, BatchMergeView, DesignerView, ...
├── Services/              # 11 service khung (TemplateScanner, WordMergeEngine, ErrorChecker, ...)
├── Repositories/          # ITemplateRepository + JsonTemplateRepository + SqliteRepository
├── Helpers/               # RelayCommand, PlaceholderRegexHelper, FileNameHelper, DialogHelper
├── Converters/            # Toàn bộ converter (color tag, status pill, multi-select, ...)
├── Resources/             # Colors.xaml + Styles.xaml (toàn bộ design system)
└── Data/                  # seed_templates.json, seed_placeholders.json, appsettings.json
```

## 5. Quy ước placeholder

- Chỉ dùng dạng `[TEN_TRUONG]` — KHÔNG dùng `{ }`.
- Tên field viết HOA, có thể chứa số và `_`. Tối đa 64 ký tự.
- Placeholder cho bảng hàng hóa: `[HH_STT]`, `[HH_MA]`, `[HH_TEN]`, `[HH_DVT]`, `[HH_SL]`, `[HH_DG]`, `[HH_TT]`.
- Placeholder cho nhà thầu: `[NT_TEN]`, `[NT_MST]`, `[NT_DIA_CHI]`, `[NT_DAI_DIEN]`, `[NT_CHUC_VU]`, `[NT_TAI_KHOAN]`, `[NT_NGAN_HANG]`.

## 6. Cách thêm 1 mẫu Word

1. Đặt file `*.docx` vào thư mục `Data/` hoặc đường dẫn cấu hình trong `appsettings.json`.
2. Trong code, gọi:

```csharp
var scanner = new TemplateScanner();
var fields = scanner.ScanDocxPlaceholders(@"D:\templates\HopDong.docx");
```

3. Phần 2 sẽ thêm UI "Thêm mẫu" để upload file → tự quét placeholder → lưu vào `JsonTemplateRepository`.

## 7. Quét placeholder

```csharp
var found = new TemplateScanner().ScanTextPlaceholders(@"
    Số: [SO_HOP_DONG] - Ngày: [NGAY_KY] - Bên A: [DONVI_TEN]
");
// found = [ SO_HOP_DONG, NGAY_KY, DONVI_TEN ]
```

## 8. Sinh thử bộ hồ sơ

### 8.1. Demo end-to-end không cần Word
Trong màn **Trộn bộ hồ sơ**, nhấn nút **"Demo: Tạo mẫu + sinh thử"**:
1. `SampleTemplateBuilder` tạo file `HopDongMuaBan_Template.docx` trong `%TEMP%\VToolPro_Demo\`.
2. `BatchMergeService` chạy:
   - `DynamicTableMerger.MergeTables` — clone 3 dòng hàng hóa (M001/M002/M003).
   - `ConditionalBlockProcessor.ApplyConditionalBlocks` — vì `[HAS_VAT]=true` nên giữ Điều 2.
   - `WordMergeEngine.ReplacePlaceholders` — thay toàn bộ placeholder scalar (an toàn cho run bị tách).
3. Mở thư mục `%TEMP%\VToolPro_Demo\` để xem file `HS001_GoiThau01_HopDongMuaBan_Demo.docx`.

### 8.2. Sinh thực tế từ mẫu của bạn
Đặt `FilePath` cho từng `TemplateItem` trong `JsonTemplateRepository`/SQLite, sau đó nhấn **"Sinh bộ hồ sơ"** ở topbar hoặc trong màn Trộn bộ hồ sơ. Engine sẽ chạy đúng pipeline trên đối với mỗi mẫu được tích chọn.

### 8.3. Xuất PDF
Khi chọn `PDF` hoặc `DOCX + PDF`, `WordMergeEngine.ExportToPdf` gọi Microsoft Word qua COM late-bound (`Type.GetTypeFromProgID("Word.Application")`). Yêu cầu: Microsoft Word đã cài. Nếu Word không có, file DOCX vẫn được tạo và một cảnh báo được ghi log.

## 9. Lộ trình các phần tiếp theo

- **Phần 2** — Engine trộn Word thật (OpenXML + xử lý split-runs).
- **Phần 3** — Bảng động (DynamicTableMerger với dòng template + lặp dòng + tổng cộng).
- **Phần 4** — Khối điều kiện (ConditionalBlockProcessor).
- **Phần 5** — Mapping engine + nối SQLite + Excel.
- **Phần 6** — Preview/Export PDF qua Microsoft Word Interop.
- **Phần 7** — Lịch sử/audit/log đầy đủ + export-import placeholder.
- **Phần 8** — Đóng gói installer.

## 10. Lưu ý pháp lý

- Phần mềm chạy hoàn toàn local/offline.
- Không sao chép thương hiệu hay tài sản phần mềm bên thứ ba.
- Tên `VToolPro Merge` là tên tạm dùng nội bộ.
