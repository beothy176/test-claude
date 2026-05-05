using System.Windows;
using System.Windows.Controls;

namespace VToolProMerge.Helpers;

/// <summary>
/// Dialog nhập 1 dòng text, gọn nhẹ, không cần XAML riêng.
/// Trả về null nếu user huỷ.
/// </summary>
public static class InputDialog
{
    public static string? Prompt(string message, string title = "Nhập",
        string defaultValue = "", Window? owner = null)
    {
        var win = new Window
        {
            Title = title,
            Width = 460,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner ?? Application.Current?.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            SizeToContent = SizeToContent.Manual,
        };

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10),
        };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var box = new TextBox
        {
            Text = defaultValue,
            Padding = new Thickness(8, 6, 8, 6),
            VerticalAlignment = VerticalAlignment.Top,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 13,
        };
        Grid.SetRow(box, 1);
        grid.Children.Add(box);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0),
        };
        var btnOk = new Button
        {
            Content = "OK",
            Width = 90,
            Height = 32,
            IsDefault = true,
            Margin = new Thickness(0, 0, 8, 0),
        };
        var btnCancel = new Button
        {
            Content = "Huỷ",
            Width = 90,
            Height = 32,
            IsCancel = true,
        };
        actions.Children.Add(btnOk);
        actions.Children.Add(btnCancel);
        Grid.SetRow(actions, 2);
        grid.Children.Add(actions);

        string? result = null;
        btnOk.Click += (_, _) => { result = box.Text; win.DialogResult = true; };

        win.Content = grid;
        win.Loaded += (_, _) => { box.SelectAll(); box.Focus(); };
        return win.ShowDialog() == true ? result : null;
    }
}
