using LapKeys.Models;
using LapKeys.Native;

namespace LapKeys.Services;

/// <summary>
/// Service for managing display refresh rates using Windows API.
/// Similar to QRes functionality.
/// </summary>
public static class DisplayService
{
    /// <summary>
    /// Gets the current display mode.
    /// </summary>
    public static DisplayMode? GetCurrentDisplayMode()
    {
        var devMode = DEVMODE.Create();

        if (NativeMethods.EnumDisplaySettingsW(null, NativeMethods.ENUM_CURRENT_SETTINGS, ref devMode))
        {
            return new DisplayMode
            {
                Width = devMode.dmPelsWidth,
                Height = devMode.dmPelsHeight,
                RefreshRate = devMode.dmDisplayFrequency,
                BitsPerPixel = devMode.dmBitsPerPel
            };
        }

        return null;
    }

    /// <summary>
    /// Gets all available display modes for the current display.
    /// </summary>
    public static List<DisplayMode> GetAllDisplayModes()
    {
        var modes = new List<DisplayMode>();
        var devMode = DEVMODE.Create();
        int modeIndex = 0;

        while (NativeMethods.EnumDisplaySettingsW(null, modeIndex++, ref devMode))
        {
            var mode = new DisplayMode
            {
                Width = devMode.dmPelsWidth,
                Height = devMode.dmPelsHeight,
                RefreshRate = devMode.dmDisplayFrequency,
                BitsPerPixel = devMode.dmBitsPerPel
            };

            // Avoid duplicates
            if (!modes.Contains(mode))
            {
                modes.Add(mode);
            }
        }

        return modes;
    }

    /// <summary>
    /// Gets available refresh rates for the current resolution.
    /// </summary>
    public static List<int> GetAvailableRefreshRates()
    {
        var currentMode = GetCurrentDisplayMode();
        if (currentMode == null)
            return new List<int>();

        return GetAvailableRefreshRates(currentMode.Width, currentMode.Height);
    }

    /// <summary>
    /// Gets available refresh rates for a specific resolution.
    /// </summary>
    public static List<int> GetAvailableRefreshRates(int width, int height)
    {
        var allModes = GetAllDisplayModes();
        
        return allModes
            .Where(m => m.Width == width && m.Height == height)
            .Select(m => m.RefreshRate)
            .Distinct()
            .OrderBy(r => r)
            .ToList();
    }

    /// <summary>
    /// Sets the display refresh rate while maintaining current resolution.
    /// </summary>
    public static bool SetRefreshRate(int refreshRate)
    {
        var currentMode = GetCurrentDisplayMode();
        if (currentMode == null)
            return false;

        return SetDisplayMode(currentMode.Width, currentMode.Height, refreshRate);
    }

    /// <summary>
    /// Sets the display mode to the specified resolution and refresh rate.
    /// </summary>
    public static bool SetDisplayMode(int width, int height, int refreshRate)
    {
        var devMode = DEVMODE.Create();

        // Get current settings as base
        if (!NativeMethods.EnumDisplaySettingsW(null, NativeMethods.ENUM_CURRENT_SETTINGS, ref devMode))
            return false;

        // Set the new values
        devMode.dmPelsWidth = width;
        devMode.dmPelsHeight = height;
        devMode.dmDisplayFrequency = refreshRate;
        devMode.dmFields = NativeMethods.DM_PELSWIDTH | 
                           NativeMethods.DM_PELSHEIGHT | 
                           NativeMethods.DM_DISPLAYFREQUENCY;

        // Test if the mode is valid first
        int testResult = NativeMethods.ChangeDisplaySettingsExW(
            null, ref devMode, IntPtr.Zero, NativeMethods.CDS_TEST, IntPtr.Zero);

        if (testResult != NativeMethods.DISP_CHANGE_SUCCESSFUL)
            return false;

        // Apply the change
        int result = NativeMethods.ChangeDisplaySettingsExW(
            null, ref devMode, IntPtr.Zero, NativeMethods.CDS_UPDATEREGISTRY, IntPtr.Zero);

        return result == NativeMethods.DISP_CHANGE_SUCCESSFUL;
    }

    /// <summary>
    /// Cycles to the next available refresh rate for the current resolution.
    /// Returns the new refresh rate, or -1 if cycling failed.
    /// </summary>
    public static int CycleRefreshRate()
    {
        var currentMode = GetCurrentDisplayMode();
        if (currentMode == null)
            return -1;

        var availableRates = GetAvailableRefreshRates();
        if (availableRates.Count <= 1)
            return currentMode.RefreshRate;

        // Find current rate index
        int currentIndex = availableRates.IndexOf(currentMode.RefreshRate);
        
        // Calculate next index (wrap around)
        int nextIndex = (currentIndex + 1) % availableRates.Count;
        int nextRate = availableRates[nextIndex];

        // Apply the new rate
        if (SetRefreshRate(nextRate))
            return nextRate;

        return -1;
    }
}
