using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace LapKeys.Services;

public class ThemeService
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public enum Theme
    {
        Light,
        Dark
    }

    private Theme _currentTheme = Theme.Light;

    public Theme CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                ApplyTheme(value);
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<Theme>? ThemeChanged;

    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
    }

    public void ApplyTheme(Theme theme)
    {
        var resources = System.Windows.Application.Current.Resources;
        bool isDark = theme == Theme.Dark;

        if (isDark)
        {
            resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
            resources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D));
            resources["CardBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
            resources["AccentBrush"] = new SolidColorBrush(Color.FromRgb(0x60, 0xCD, 0xFF));
            resources["AccentHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x4C, 0xB8, 0xE8));
            resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
            resources["TextTertiaryBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            resources["SubtleBrush"] = new SolidColorBrush(Color.FromRgb(0x38, 0x38, 0x38));
            resources["SubtleHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x45, 0x45, 0x45));
        }
        else
        {
            resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3));
            resources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            resources["CardBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xE5));
            resources["AccentBrush"] = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
            resources["AccentHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x10, 0x6E, 0xBE));
            resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));
            resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            resources["TextTertiaryBrush"] = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));
            resources["SubtleBrush"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
            resources["SubtleHoverBrush"] = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8));
        }

        if (System.Windows.Application.Current != null)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                ApplyWindowDarkMode(window, isDark);
            }
        }
    }

    public void Initialize()
    {
        if (System.Windows.Application.Current != null)
        {
            System.Windows.Application.Current.MainWindow.SourceInitialized += (s, e) =>
            {
                ApplyWindowDarkMode((Window)s!, CurrentTheme == Theme.Dark);
            };
        }
    }

    private static void ApplyWindowDarkMode(Window window, bool isDark)
    {
        try
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero) return;

            int useImmersiveDarkMode = isDark ? 1 : 0;
            
            // Try Windows 11 / Windows 10 20H1+
            int hr = DwmSetWindowAttribute(helper.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            if (hr != 0)
            {
                // Try older Windows 10 build 1809+
                DwmSetWindowAttribute(helper.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
            }
        }
        catch
        {
            // Ignore if DWM is not available
        }
    }
}
