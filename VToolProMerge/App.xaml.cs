using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace VToolProMerge;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        base.OnStartup(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Show(e.Exception, "UI thread");
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex) Show(ex, "Background thread");
    }

    private static void Show(Exception ex, string source)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Lỗi không xử lý ({source}):");
        sb.AppendLine();
        for (var x = ex; x != null; x = x.InnerException)
        {
            sb.AppendLine($"[{x.GetType().Name}] {x.Message}");
        }
        sb.AppendLine();
        sb.AppendLine("Stack:");
        sb.AppendLine(ex.ToString());
        MessageBox.Show(sb.ToString(), "VToolPro Merge — Crash",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
