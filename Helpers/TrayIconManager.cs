using System.Drawing;
using System.IO;
using System.Reflection;
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
        // Try to load the application icon, fallback to system icon
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var iconStream = assembly.GetManifestResourceStream("LapKeys.app.ico");
            if (iconStream != null)
            {
                return new Icon(iconStream);
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
