using System.Windows;
using System.Windows.Interop;
using LapKeys.Native;

namespace LapKeys.Helpers;

/// <summary>
/// Positions overlay windows on a specific monitor's work area, accounting for per-monitor DPI.
/// </summary>
public static class OverlayPositioner
{
    /// <summary>
    /// Marks the window as non-activating so showing it never steals focus from the foreground app.
    /// Call once after the HWND exists (e.g. from SourceInitialized).
    /// </summary>
    public static void MakeNonActivating(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
            return;

        int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        exStyle |= NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW;
        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, exStyle);
    }

    /// <summary>
    /// Places the window centered horizontally and near the bottom of the target monitor's work area.
    /// When <paramref name="deviceName"/> is null/empty or not found, falls back to the primary monitor.
    /// </summary>
    public static void PositionBottomCenter(Window window, string? deviceName)
    {
        NativeMethods.RECT workArea = new NativeMethods.RECT
        {
            left = 0,
            top = 0,
            right = (int)SystemParameters.WorkArea.Width,
            bottom = (int)SystemParameters.WorkArea.Height
        };

        if (!string.IsNullOrEmpty(deviceName))
        {
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
                {
                    var mi = new NativeMethods.MONITORINFOEX();
                    mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                    if (NativeMethods.GetMonitorInfo(hMonitor, ref mi))
                    {
                        if (mi.szDevice.TrimEnd('\0') == deviceName)
                        {
                            workArea = mi.rcWork;
                            return false; // stop enumerating
                        }
                    }
                    return true;
                }, IntPtr.Zero);
        }

        // Convert physical pixels (Win32 RECT) to logical units (WPF) using this window's DPI.
        var source = PresentationSource.FromVisual(window);
        double dpiX = 1.0, dpiY = 1.0;
        if (source?.CompositionTarget != null)
        {
            dpiX = source.CompositionTarget.TransformFromDevice.M11;
            dpiY = source.CompositionTarget.TransformFromDevice.M22;
        }

        double workAreaWidth = (workArea.right - workArea.left) * dpiX;
        double workAreaHeight = (workArea.bottom - workArea.top) * dpiY;
        double workAreaLeft = workArea.left * dpiX;
        double workAreaTop = workArea.top * dpiY;

        window.Left = workAreaLeft + (workAreaWidth - window.Width) / 2;
        window.Top = workAreaTop + workAreaHeight - window.Height - 60;
    }
}
