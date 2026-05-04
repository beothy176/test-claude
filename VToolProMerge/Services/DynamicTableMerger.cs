using System.Data;
using System.Globalization;

namespace VToolProMerge.Services;

/// <summary>
/// Merge bảng động (lặp dòng) vào Word. Bản khung — sẽ nối với OpenXML/Interop.
/// </summary>
public class DynamicTableMerger
{
    public void MergeTable(string docxPath, string tableName, DataTable rows)
    {
        // TODO: Tìm bảng có name (tag) = tableName trong DOCX và:
        //   1) Lưu lại dòng template (chứa các [HH_*])
        //   2) Clone dòng template cho từng row dữ liệu
        //   3) Replace placeholder bằng giá trị tương ứng
        //   4) Xóa dòng template gốc
        //   5) Nếu có dòng tổng cộng -> thay [TONG_TIEN]
        // Yêu cầu: giữ định dạng border, font, alignment gốc.
    }

    public decimal CalculateTotal(DataTable rows, string columnName)
    {
        decimal sum = 0;
        foreach (DataRow r in rows.Rows)
        {
            if (decimal.TryParse(r[columnName]?.ToString(), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var v)) sum += v;
        }
        return sum;
    }

    public decimal ApplyVat(decimal amount, decimal vatPercent) => amount + amount * vatPercent / 100m;

    public string FormatNumber(decimal v, string format = "#,##0.##")
        => v.ToString(format, CultureInfo.GetCultureInfo("vi-VN"));

    public void PreserveWordTableFormat()
    {
        // Mọi thao tác clone row phải đi theo cây OpenXML để giữ TableProperties + run formatting.
    }
}
