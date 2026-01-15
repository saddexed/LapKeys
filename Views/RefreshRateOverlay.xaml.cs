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
        
        PositionOverlay();
    }

    private void PositionOverlay()
    {
        var screen = SystemParameters.WorkArea;
        Left = (screen.Width - Width) / 2;
        Top = screen.Bottom - Height - 60;
    }

    public void ShowRefreshRate(int refreshRate)
    {
        RateText.Text = refreshRate.ToString();
        
        _hideTimer.Stop();
        _hideTimer.Start();
        
        Show();
        
        PositionOverlay();
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
