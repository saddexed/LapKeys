using LapKeys.Models;
using LapKeys.Native;

namespace LapKeys.Services;

public static class DisplayService
{
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

            if (!modes.Contains(mode))
            {
                modes.Add(mode);
            }
        }

        return modes;
    }

    public static List<int> GetAvailableRefreshRates()
    {
        var currentMode = GetCurrentDisplayMode();
        if (currentMode == null)
            return new List<int>();

        return GetAvailableRefreshRates(currentMode.Width, currentMode.Height);
    }

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

    public static bool SetRefreshRate(int refreshRate)
    {
        var currentMode = GetCurrentDisplayMode();
        if (currentMode == null)
            return false;

        return SetDisplayMode(currentMode.Width, currentMode.Height, refreshRate);
    }

    public static bool SetDisplayMode(int width, int height, int refreshRate)
    {
        var devMode = DEVMODE.Create();

        if (!NativeMethods.EnumDisplaySettingsW(null, NativeMethods.ENUM_CURRENT_SETTINGS, ref devMode))
            return false;

        devMode.dmPelsWidth = width;
        devMode.dmPelsHeight = height;
        devMode.dmDisplayFrequency = refreshRate;
        devMode.dmFields = NativeMethods.DM_PELSWIDTH | 
                           NativeMethods.DM_PELSHEIGHT | 
                           NativeMethods.DM_DISPLAYFREQUENCY;

        int testResult = NativeMethods.ChangeDisplaySettingsExW(
            null, ref devMode, IntPtr.Zero, NativeMethods.CDS_TEST, IntPtr.Zero);

        if (testResult != NativeMethods.DISP_CHANGE_SUCCESSFUL)
            return false;

        int result = NativeMethods.ChangeDisplaySettingsExW(
            null, ref devMode, IntPtr.Zero, NativeMethods.CDS_UPDATEREGISTRY, IntPtr.Zero);

        return result == NativeMethods.DISP_CHANGE_SUCCESSFUL;
    }

    public static int CycleRefreshRate()
    {
        return CycleRefreshRate(null);
    }

    public static int CycleRefreshRate(List<int>? allowedRates)
    {
        var currentMode = GetCurrentDisplayMode();
        if (currentMode == null)
            return -1;

        var availableRates = allowedRates ?? GetAvailableRefreshRates();
        if (availableRates.Count == 0)
            return currentMode.RefreshRate;
        
        if (availableRates.Count == 1)
            return availableRates[0];

        availableRates = availableRates.OrderBy(r => r).ToList();

        int currentIndex = availableRates.IndexOf(currentMode.RefreshRate);
        
        if (currentIndex < 0)
            currentIndex = -1;
        
        int nextIndex = (currentIndex + 1) % availableRates.Count;
        int nextRate = availableRates[nextIndex];

        if (SetRefreshRate(nextRate))
            return nextRate;

        return -1;
    }
}
