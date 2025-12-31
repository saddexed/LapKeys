using System.Windows;
using System.Windows.Threading;

namespace LapKeys.Views;

/// <summary>
/// A Windows-style brightness overlay that appears briefly when brightness changes.
/// </summary>
public partial class BrightnessOverlay : Window
{
    private readonly DispatcherTimer _hideTimer;
    private static BrightnessOverlay? _instance;

    public BrightnessOverlay()
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
        
        // Position at bottom center of primary screen
        PositionOverlay();
    }

    private void PositionOverlay()
    {
        var screen = SystemParameters.WorkArea;
        Left = (screen.Width - Width) / 2;
        Top = screen.Bottom - Height - 60;
    }

    /// <summary>
    /// Shows the overlay with the specified brightness level.
    /// </summary>
    public void ShowBrightness(int brightness)
    {
        brightness = Math.Clamp(brightness, 0, 100);
        
        // Update percentage text
        PercentText.Text = $"{brightness}%";
        
        // Reset and start hide timer
        _hideTimer.Stop();
        _hideTimer.Start();
        
        // Show the overlay
        Show();
        
        // Ensure position is correct
        PositionOverlay();
        
        // Update progress bar width after layout is updated
        Dispatcher.BeginInvoke(new Action(() =>
        {
            double containerWidth = ProgressBarContainer.ActualWidth;
            if (containerWidth > 0)
            {
                ProgressFill.Width = (brightness / 100.0) * containerWidth;
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Gets the singleton instance of the brightness overlay.
    /// </summary>
    public static BrightnessOverlay Instance
    {
        get
        {
            if (_instance == null || !_instance.IsLoaded)
            {
                _instance = new BrightnessOverlay();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Shows the brightness overlay with the specified level.
    /// </summary>
    public static void ShowOverlay(int brightness)
    {
        Instance.ShowBrightness(brightness);
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
