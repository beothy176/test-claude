using System.Windows;

namespace VToolProMerge.Helpers;

public static class DialogHelper
{
    public static void Info(string message, string title = "Thông báo")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public static void Warn(string message, string title = "Cảnh báo")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public static void Error(string message, string title = "Lỗi")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public static bool Confirm(string message, string title = "Xác nhận")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;
}
