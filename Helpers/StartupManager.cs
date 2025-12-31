using Microsoft.Win32;
using System.Reflection;

namespace LapKeys.Helpers;

/// <summary>
/// Manages the Windows startup registry entry for the application.
/// </summary>
public static class StartupManager
{
    private const string AppName = "LapKeys";
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Gets or sets whether the application runs at Windows startup.
    /// </summary>
    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }
        set
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null) return;

                if (value)
                {
                    var exePath = GetExecutablePath();
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // Add --minimized argument to start in tray
                        key.SetValue(AppName, $"\"{exePath}\" --minimized");
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch
            {
                // Silently fail if we can't modify the registry
            }
        }
    }

    /// <summary>
    /// Gets the path to the executable.
    /// </summary>
    private static string? GetExecutablePath()
    {
        // Get the main module path (works for both debug and release)
        var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
        return mainModule?.FileName;
    }
}
