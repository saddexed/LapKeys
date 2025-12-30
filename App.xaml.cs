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

        // Create and show main window
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _trayIconManager?.Dispose();
    }
}

