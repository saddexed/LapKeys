using System.Windows;
using System.Windows.Threading;
using LapKeys.Helpers;
using LapKeys.Services;

namespace LapKeys;

public partial class App : System.Windows.Application
{
    private TrayIconManager? _trayIconManager;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Log unhandled exceptions from the UI thread and background threads.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

        LogService.Info("LapKeys starting");

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

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogService.Error("Unhandled UI exception", e.Exception);
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogService.Error("Unhandled exception", ex);
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        LogService.Info("LapKeys exiting");
        _trayIconManager?.Dispose();
    }
}

