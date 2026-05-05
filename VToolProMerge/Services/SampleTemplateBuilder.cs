using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace VToolProMerge.Services;

/// <summary>
/// Tạo template DOCX mẫu bằng OpenXML để demo end-to-end mà không cần Word.
/// Bao gồm: placeholder scalar, khối điều kiện [IF:HAS_VAT]...[ENDIF:HAS_VAT],
/// và bảng động prefix HH (Hàng hóa).
/// </summary>
public static class SampleTemplateBuilder
{
    public static string CreateContractTemplate(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var path = Path.Combine(outputDir, "HopDongMuaBan_Template.docx");

        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document(new Body(BuildBody().ToArray()));
        main.Document.Save();
        return path;
    }

    private static IEnumerable<OpenXmlElement> BuildBody()
    {
        yield return Para("[DONVI_TEN]", bold: true, alignCenter: true);
        yield return Para("Số: [SO_HOP_DONG]", alignCenter: true);
        yield return Para("");
        yield return Para("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", bold: true, alignCenter: true);
        yield return Para("Độc lập - Tự do - Hạnh phúc", bold: true, alignCenter: true);
        yield return Para("");
        yield return Para("HỢP ĐỒNG MUA BÁN HÀNG HÓA", bold: true, size: 28, alignCenter: true);
        yield return Para("-----o0o-----", alignCenter: true);
        yield return Para("");
        yield return Para("Hôm nay, ngày [NGAY_KY], tại [DONVI_TEN], chúng tôi gồm có:");
        yield return Para("BÊN A (BÊN MUA):", bold: true);
        yield return Para("Tên đơn vị     : [DONVI_TEN]");
        yield return Para("Mã số thuế     : [DONVI_MST]");
        yield return Para("Địa chỉ        : [DONVI_DIA_CHI]");
        yield return Para("Người đại diện : [DONVI_NGUOI_DD]");
        yield return Para("");
        yield return Para("BÊN B (BÊN BÁN):", bold: true);
        yield return Para("Tên đơn vị     : [NT_TEN]");
        yield return Para("Mã số thuế     : [NT_MST]");
        yield return Para("Địa chỉ        : [NT_DIA_CHI]");
        yield return Para("Người đại diện : [NT_DAI_DIEN]");
        yield return Para("");
        yield return Para("Điều 1. Hàng hóa và giá trị hợp đồng", bold: true);
        yield return Para("Bên B đồng ý bán, Bên A đồng ý mua các mặt hàng sau:");

        // Table dynamic — header + 1 template row + total row
        yield return BuildHangHoaTable();

        yield return Para("");
        yield return Para("[IF:HAS_VAT]");
        yield return Para("Điều 2. Thuế GTGT", bold: true);
        yield return Para("Giá trị hợp đồng đã bao gồm thuế GTGT theo quy định hiện hành.");
        yield return Para("[ENDIF:HAS_VAT]");
        yield return Para("");
        yield return Para("Điều 3. Điều khoản chung", bold: true);
        yield return Para("Hợp đồng có hiệu lực từ ngày ký.");
    }

    private static Table BuildHangHoaTable()
    {
        var table = new Table();

        // Border
        var props = new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
            ),
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct });
        table.AppendChild(props);

        // Header row
        table.AppendChild(BuildRow(true, "STT", "Mã hàng", "Tên hàng", "ĐVT", "Số lượng", "Đơn giá", "Thành tiền"));
        // Template row (chứa các [HH_*] placeholders)
        table.AppendChild(BuildRow(false, "[HH_STT]", "[HH_MA]", "[HH_TEN]", "[HH_DVT]",
            "[HH_SL]", "[HH_DG]", "[HH_TT]"));
        // Total row (placeholder scalar [TONG_TIEN])
        var totalRow = new TableRow();
        var labelCell = new TableCell(new Paragraph(new Run(new Text("Tổng cộng:"))));
        labelCell.AppendChild(new TableCellProperties(new GridSpan { Val = 6 }));
        totalRow.AppendChild(labelCell);
        totalRow.AppendChild(new TableCell(new Paragraph(new Run(new Text("[TONG_TIEN]")))));
        table.AppendChild(totalRow);

        return table;
    }

    private static TableRow BuildRow(bool bold, params string[] cells)
    {
        var row = new TableRow();
        foreach (var c in cells)
        {
            var run = new Run(new Text(c) { Space = SpaceProcessingModeValues.Preserve });
            if (bold) run.PrependChild(new RunProperties(new Bold()));
            row.AppendChild(new TableCell(new Paragraph(run)));
        }
        return row;
    }

    private static Paragraph Para(string text, bool bold = false, bool alignCenter = false, int? size = null)
    {
        var p = new Paragraph();
        if (alignCenter)
        {
            p.AppendChild(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
        }
        var run = new Run();
        var rp = new RunProperties();
        if (bold) rp.AppendChild(new Bold());
        if (size.HasValue) rp.AppendChild(new FontSize { Val = size.Value.ToString() });
        rp.AppendChild(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        run.AppendChild(rp);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        p.AppendChild(run);
        return p;
    }
}
