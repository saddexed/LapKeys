using System.Windows;
using System.Windows.Threading;

namespace LapKeys.Views;

public partial class RefreshRateOverlay : Window
{
    private readonly DispatcherTimer _hideTimer;
    private static RefreshRateOverlay? _instance;

    public RefreshRateOverlay()
    {
        InitializeComponent();
        
        _hideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _hideTimer.Tick += (s, e) =>
        {
            _hideTimer.Stop();
            Hide();
        };
    }

    private void PositionOverlay(string? deviceName)
    {
        Native.NativeMethods.RECT targetWorkArea = new Native.NativeMethods.RECT 
        { 
            left = 0, top = 0, 
            right = (int)SystemParameters.WorkArea.Width, 
            bottom = (int)SystemParameters.WorkArea.Height 
        };

        if (!string.IsNullOrEmpty(deviceName))
        {
            Native.NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Native.NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
            {
                var mi = new Native.NativeMethods.MONITORINFOEX();
                mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Native.NativeMethods.MONITORINFOEX));
                if (Native.NativeMethods.GetMonitorInfo(hMonitor, ref mi))
                {
                    if (mi.szDevice.TrimEnd('\0') == deviceName)
                    {
                        targetWorkArea = mi.rcWork;
                        return false; // Stop enumerating
                    }
                }
                return true;
            }, IntPtr.Zero);
        }

        // DPIScaling might be needed, but WPF usually handles DPI. 
        // We might need to convert physical pixels to logical pixels if the monitor has different DPI.
        // For simplicity, we just use the RECT directly assuming primary DPI, or convert using PresentationSource.
        var source = PresentationSource.FromVisual(this);
        double dpiX = 1.0, dpiY = 1.0;
        if (source?.CompositionTarget != null)
        {
            dpiX = source.CompositionTarget.TransformFromDevice.M11;
            dpiY = source.CompositionTarget.TransformFromDevice.M22;
        }

        double workAreaWidth = (targetWorkArea.right - targetWorkArea.left) * dpiX;
        double workAreaHeight = (targetWorkArea.bottom - targetWorkArea.top) * dpiY;
        double workAreaLeft = targetWorkArea.left * dpiX;
        double workAreaTop = targetWorkArea.top * dpiY;

        Left = workAreaLeft + (workAreaWidth - Width) / 2;
        Top = workAreaTop + workAreaHeight - Height - 60;
    }

    public void ShowRefreshRate(int refreshRate, string? deviceName = null)
    {
        RateText.Text = refreshRate.ToString();
        
        _hideTimer.Stop();
        _hideTimer.Start();
        
        Show();
        
        PositionOverlay(deviceName);
    }

    public static RefreshRateOverlay Instance
    {
        get
        {
            if (_instance == null || !_instance.IsLoaded)
            {
                _instance = new RefreshRateOverlay();
            }
            return _instance;
        }
    }

    public static void ShowOverlay(int refreshRate, string? deviceName = null)
    {
        Instance.ShowRefreshRate(refreshRate, deviceName);
    }

    protected override void OnClosed(EventArgs e)
    {
        _hideTimer.Stop();
        if (_instance == this)
        {
            _instance = null;
        }
        base.OnClosed(e);
    }
}
