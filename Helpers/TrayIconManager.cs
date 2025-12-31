using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace LapKeys.Helpers;

public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private bool _disposed;

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = GetApplicationIcon(),
            Visible = true,
            Text = "LapKeys - Laptop Control"
        };

        // Create context menu
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show", null, OnShow);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, OnExit);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += OnShow;
    }

    private Icon GetApplicationIcon()
    {
        // Try to load the application icon from WPF resource
        try
        {
            var resourceUri = new Uri("pack://application:,,,/app.ico", UriKind.Absolute);
            var streamInfo = System.Windows.Application.GetResourceStream(resourceUri);
            if (streamInfo != null)
            {
                return new Icon(streamInfo.Stream);
            }
        }
        catch
        {
            // Fallback to default
        }

        return SystemIcons.Application;
    }

    private void OnShow(object? sender, EventArgs e)
    {
        var mainWindow = WpfApplication.Current.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        WpfApplication.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _notifyIcon?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
