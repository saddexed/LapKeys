using LapKeys.Models;
using LapKeys.Native;

namespace LapKeys.Services;

public static class DisplayService
{
    public static List<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();
        var d = new DISPLAY_DEVICE();
        d.cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf(d);
        uint id = 0;
        int index = 1;
        while (NativeMethods.EnumDisplayDevices(null, id, ref d, 0))
        {
            if ((d.StateFlags & 0x01) != 0) // DISPLAY_DEVICE_ATTACHED_TO_DESKTOP
            {
                monitors.Add(new MonitorInfo
                {
                    DeviceName = d.DeviceName.Split('\0')[0],
                    DisplayName = d.DeviceString.Split('\0')[0],
                    Index = index++
                });
            }
            d.cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf(d);
            id++;
        }
        return monitors;
    }

    public static DisplayMode? GetCurrentDisplayMode(string? deviceName = null)
    {
        var devMode = DEVMODE.Create();

        if (NativeMethods.EnumDisplaySettingsW(deviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref devMode))
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

    public static List<DisplayMode> GetAllDisplayModes(string? deviceName = null)
    {
        var modes = new List<DisplayMode>();
        var devMode = DEVMODE.Create();
        int modeIndex = 0;

        while (NativeMethods.EnumDisplaySettingsW(deviceName, modeIndex++, ref devMode))
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

    public static List<int> GetAvailableRefreshRates(string? deviceName = null)
    {
        var currentMode = GetCurrentDisplayMode(deviceName);
        if (currentMode == null)
            return new List<int>();

        return GetAvailableRefreshRates(currentMode.Width, currentMode.Height, deviceName);
    }

    public static List<int> GetAvailableRefreshRates(int width, int height, string? deviceName = null)
    {
        var allModes = GetAllDisplayModes(deviceName);
        
        return allModes
            .Where(m => m.Width == width && m.Height == height)
            .Select(m => m.RefreshRate)
            .Distinct()
            .OrderBy(r => r)
            .ToList();
    }

    public static bool SetRefreshRate(int refreshRate, string? deviceName = null)
    {
        var currentMode = GetCurrentDisplayMode(deviceName);
        if (currentMode == null)
            return false;

        return SetDisplayMode(currentMode.Width, currentMode.Height, refreshRate, deviceName);
    }

    public static bool SetDisplayMode(int width, int height, int refreshRate, string? deviceName = null)
    {
        var devMode = DEVMODE.Create();

        if (!NativeMethods.EnumDisplaySettingsW(deviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref devMode))
            return false;

        devMode.dmPelsWidth = width;
        devMode.dmPelsHeight = height;
        devMode.dmDisplayFrequency = refreshRate;
        devMode.dmFields = NativeMethods.DM_PELSWIDTH | 
                           NativeMethods.DM_PELSHEIGHT | 
                           NativeMethods.DM_DISPLAYFREQUENCY;

        int testResult = NativeMethods.ChangeDisplaySettingsExW(
            deviceName, ref devMode, IntPtr.Zero, NativeMethods.CDS_TEST, IntPtr.Zero);

        if (testResult != NativeMethods.DISP_CHANGE_SUCCESSFUL)
            return false;

        int result = NativeMethods.ChangeDisplaySettingsExW(
            deviceName, ref devMode, IntPtr.Zero, NativeMethods.CDS_UPDATEREGISTRY, IntPtr.Zero);

        return result == NativeMethods.DISP_CHANGE_SUCCESSFUL;
    }

    public static int CycleRefreshRate(string? deviceName = null)
    {
        return CycleRefreshRate(null, deviceName);
    }

    public static int CycleRefreshRate(List<int>? allowedRates, string? deviceName = null)
    {
        var currentMode = GetCurrentDisplayMode(deviceName);
        if (currentMode == null)
            return -1;

        var availableRates = allowedRates ?? GetAvailableRefreshRates(deviceName);
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

        if (SetRefreshRate(nextRate, deviceName))
            return nextRate;

        return -1;
    }
}
