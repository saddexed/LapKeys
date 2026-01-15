using System.Windows;
using System.Windows.Threading;

namespace LapKeys.Views;

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
        
        PositionOverlay();
    }

    private void PositionOverlay()
    {
        var screen = SystemParameters.WorkArea;
        Left = (screen.Width - Width) / 2;
        Top = screen.Bottom - Height - 60;
    }

    public void ShowBrightness(int brightness)
    {
        brightness = Math.Clamp(brightness, 0, 100);
        
        PercentText.Text = $"{brightness}%";
        
        _hideTimer.Stop();
        _hideTimer.Start();
        
        Show();
        
        PositionOverlay();
        
        Dispatcher.BeginInvoke(new Action(() =>
        {
            double containerWidth = ProgressBarContainer.ActualWidth;
            if (containerWidth > 0)
            {
                ProgressFill.Width = (brightness / 100.0) * containerWidth;
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

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
