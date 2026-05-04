# VToolPro Merge — Phần 1 (UI khung chạy được)

Phần mềm trộn thư / sinh hồ sơ chuyên nghiệp, chạy local/offline trên Windows.
Phần 1 này tạo project WPF .NET 8 chạy được với MainWindow + Sidebar + Topbar +
Statusbar và 3 màn hình chính theo đúng tinh thần 3 ảnh giao diện đính kèm:

1. **Thư viện mẫu** — hiển thị card mẫu, filter theo nhóm, panel "Thông tin mẫu" + "Tóm tắt placeholder".
2. **Trộn bộ hồ sơ** — 5 thẻ thống kê + 4 cột (Chọn mẫu / Dữ liệu & kiểm tra / Kết quả đầu ra / Xem trước & lịch sử) + tiến trình 4 bước + nút "Sinh bộ hồ sơ".
3. **Thiết kế mẫu (Designer)** — breadcrumb, toolbar, format bar, trang Word A4 mô phỏng có placeholder `[TEN_TRUONG]`, tab strip dọc + 4 panel (Nguồn dữ liệu / Cây phần tử / Quick Builder / Thuộc tính).

> Phần 2 sẽ nối engine thật với Word/Excel (OpenXML + Microsoft Office Interop).

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

Tại màn **Trộn bộ hồ sơ**, nhấn nút lớn "Sinh bộ hồ sơ".
Phiên bản hiện tại chạy giả lập tiến trình 4 bước để bạn thấy luồng.
Phần 2 sẽ nối với `BatchMergeService.GenerateDocuments(...)` thực tế.

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
