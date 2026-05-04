using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VToolProMerge.Models;

namespace VToolProMerge.Converters;

/// <summary>True -> Visible, False -> Collapsed</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>False -> Visible</summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v != Visibility.Visible;
}

/// <summary>So sánh string với parameter -> bool</summary>
public class StringEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b && b) ? parameter ?? Binding.DoNothing : Binding.DoNothing;
}

/// <summary>So sánh -> Visibility</summary>
public class StringEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal)
           ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>Trả về Brush tag theo Category mẫu.</summary>
public class CategoryToTagBrushConverter : IValueConverter
{
    public bool ReturnTextBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var cat = value?.ToString() ?? string.Empty;
        var key = cat switch
        {
            "Báo giá"     => ReturnTextBrush ? "TagBaoGiaTextColor"     : "TagBaoGiaColor",
            "Hợp đồng"    => ReturnTextBrush ? "TagHopDongTextColor"    : "TagHopDongColor",
            "Đấu thầu"    => ReturnTextBrush ? "TagDauThauTextColor"    : "TagDauThauColor",
            "Báo cáo"     => ReturnTextBrush ? "TagBaoCaoTextColor"     : "TagBaoCaoColor",
            "Quyết định"  => ReturnTextBrush ? "TagQuyetDinhTextColor"  : "TagQuyetDinhColor",
            "Tờ trình"    => ReturnTextBrush ? "TagToTrinhTextColor"    : "TagToTrinhColor",
            "Danh mục"    => ReturnTextBrush ? "TagDanhMucTextColor"    : "TagDanhMucColor",
            _             => ReturnTextBrush ? "TagDanhMucTextColor"    : "TagDanhMucColor"
        };
        var color = (Color)Application.Current.Resources[key];
        return new SolidColorBrush(color);
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>TemplateKind -> nền icon Word/Excel.</summary>
public class TemplateKindToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TemplateKind k && k == TemplateKind.Excel)
            return Application.Current.Resources["ExcelGreenBrush"];
        return Application.Current.Resources["WordBlueBrush"];
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>TemplateKind -> chữ "W" / "X"</summary>
public class TemplateKindToLetterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is TemplateKind k && k == TemplateKind.Excel) ? "X" : "W";
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>Status -> Brush nền pill</summary>
public class StatusToBrushConverter : IValueConverter
{
    public bool Soft { get; set; } = true;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value?.ToString() ?? string.Empty;
        var key = s switch
        {
            "Sẵn sàng"  => Soft ? "SuccessSoftBrush" : "SuccessBrush",
            "Hoàn tất"  => Soft ? "SuccessSoftBrush" : "SuccessBrush",
            "Cảnh báo"  => Soft ? "WarningSoftBrush" : "WarningBrush",
            "Lỗi"       => Soft ? "DangerSoftBrush"  : "DangerBrush",
            _           => Soft ? "BgChipBrush"      : "TextMutedBrush"
        };
        return Application.Current.Resources[key];
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public class IssueSeverityToBrushConverter : IValueConverter
{
    public bool Soft { get; set; } = true;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value is IssueSeverity sev ? sev : IssueSeverity.Info;
        var key = s switch
        {
            IssueSeverity.Error   => Soft ? "DangerSoftBrush"  : "DangerBrush",
            IssueSeverity.Warning => Soft ? "WarningSoftBrush" : "WarningBrush",
            _                     => Soft ? "InfoSoftBrush"    : "InfoBrush"
        };
        return Application.Current.Resources[key];
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public class IssueCountLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MergeIssue i) return string.Empty;
        return i.Severity switch
        {
            IssueSeverity.Error   => $"{i.Count} lỗi",
            IssueSeverity.Warning => $"{i.Count} cảnh báo",
            _                     => $"{i.Count}"
        };
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public class StepStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i && i >= 1)
            return Application.Current.Resources["BrandPrimaryBrush"];
        return Application.Current.Resources["BgChipBrush"];
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public class StepStatusToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i && i >= 1)
            return new SolidColorBrush(Colors.White);
        return Application.Current.Resources["TextMutedBrush"];
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public class IsNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value == null ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int n = value is int i ? i : 0;
        return n > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>So sánh 2 giá trị qua MultiBinding -> bool/Visibility.</summary>
public class EqualMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return false;
        return Equals(values[0], values[1]);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => System.Array.Empty<object>();
}

public class EqualMultiToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var equal = values != null && values.Length >= 2 && Equals(values[0], values[1]);
        return Application.Current.Resources[equal ? "BrandPrimaryBrush" : "BorderBrush"];
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => System.Array.Empty<object>();
}

public class EqualMultiToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var equal = values != null && values.Length >= 2 && Equals(values[0], values[1]);
        return equal ? Visibility.Visible : Visibility.Collapsed;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => System.Array.Empty<object>();
}
