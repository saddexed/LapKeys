using Microsoft.Win32;
using System.Reflection;

namespace LapKeys.Helpers;

public static class StartupManager
{
    private const string AppName = "LapKeys";
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

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
            }
        }
    }

    private static string? GetExecutablePath()
    {
        var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
        return mainModule?.FileName;
    }
}
