using System.Windows;
using System.Windows.Threading;

namespace LapKeys.Views;

/// <summary>
/// A Windows-style overlay that appears briefly when refresh rate changes.
/// </summary>
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
    /// Shows the overlay with the specified refresh rate.
    /// </summary>
    public void ShowRefreshRate(int refreshRate)
    {
        // Update rate text
        RateText.Text = refreshRate.ToString();
        
        // Reset and start hide timer
        _hideTimer.Stop();
        _hideTimer.Start();
        
        // Show the overlay
        Show();
        
        // Ensure position is correct
        PositionOverlay();
    }

    /// <summary>
    /// Gets the singleton instance of the refresh rate overlay.
    /// </summary>
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

    /// <summary>
    /// Shows the refresh rate overlay with the specified rate.
    /// </summary>
    public static void ShowOverlay(int refreshRate)
    {
        Instance.ShowRefreshRate(refreshRate);
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
