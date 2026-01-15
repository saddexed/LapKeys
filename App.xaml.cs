using System.Windows;
using LapKeys.Helpers;

namespace LapKeys;

public partial class App : System.Windows.Application
{
    private TrayIconManager? _trayIconManager;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _trayIconManager = new TrayIconManager();
        _trayIconManager.Initialize();

        bool startMinimized = e.Args.Contains("--minimized");

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        
        if (startMinimized)
        {
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

