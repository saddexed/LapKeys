using System.Windows;
using LapKeys.Helpers;

namespace LapKeys;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private TrayIconManager? _trayIconManager;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Initialize system tray
        _trayIconManager = new TrayIconManager();
        _trayIconManager.Initialize();

        // Check for --minimized argument (used when starting with Windows)
        bool startMinimized = e.Args.Contains("--minimized");

        // Create and show main window
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        
        if (startMinimized)
        {
            // Don't show window, just run in tray
            mainWindow.Hide();
        }
        else
        {
            mainWindow.Show();
        }
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _trayIconManager?.Dispose();
    }
}

