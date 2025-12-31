using System.Windows;
using LapKeys.Services;
using LapKeys.ViewModels;
using LapKeys.Views;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;
using Keyboard = System.Windows.Input.Keyboard;
using Button = System.Windows.Controls.Button;

namespace LapKeys;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly HotkeyService _hotkeyService;
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private bool _isCapturingHotkey;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        _hotkeyService = new HotkeyService();
        
        // Subscribe to ViewModel events
        ViewModel.RequestHotkeyCapture += OnRequestHotkeyCapture;
        ViewModel.RefreshRateChanged += OnRefreshRateChanged;
        ViewModel.BrightnessChanged += OnBrightnessChanged;
        ViewModel.RefreshRateHotkeyToggled += RegisterCurrentHotkey;
        ViewModel.BrightnessHotkeysToggled += RegisterCurrentHotkey;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize hotkey service after window is loaded
        _hotkeyService.Initialize(this);
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        
        // Register default hotkey
        RegisterCurrentHotkey();
    }

    private void RegisterCurrentHotkey()
    {
        _hotkeyService.UnregisterAllHotkeys();
        
        // Register refresh rate hotkey if enabled
        if (ViewModel.IsRefreshRateHotkeyEnabled)
        {
            if (_hotkeyService.RegisterHotkey(ViewModel.CycleRefreshRateHotkey))
            {
                ViewModel.StatusMessage = $"Hotkey registered: {ViewModel.CycleRefreshRateHotkey}";
            }
            else
            {
                ViewModel.StatusMessage = $"Failed to register hotkey (may be in use by another app)";
            }
        }
        
        // Register brightness hotkeys if supported and enabled
        if (ViewModel.IsBrightnessSupported && ViewModel.IsBrightnessHotkeysEnabled)
        {
            _hotkeyService.RegisterHotkey(ViewModel.BrightnessUpHotkey);
            _hotkeyService.RegisterHotkey(ViewModel.BrightnessDownHotkey);
        }
    }

    private void OnHotkeyPressed(object? sender, Models.HotkeyBinding binding)
    {
        switch (binding.Action)
        {
            case "CycleRefreshRate":
                ViewModel.ExecuteCycleRefreshRate();
                break;
            case "BrightnessUp":
                ViewModel.ExecuteIncreaseBrightness();
                break;
            case "BrightnessDown":
                ViewModel.ExecuteDecreaseBrightness();
                break;
        }
    }

    private void OnRequestHotkeyCapture()
    {
        _isCapturingHotkey = true;
        _hotkeyService.UnregisterAllHotkeys(); // Temporarily unregister to capture
        Focus();
    }

    private void OnRefreshRateChanged(int newRate)
    {
        RefreshRateOverlay.ShowOverlay(newRate);
    }

    private void OnBrightnessChanged(int brightness)
    {
        BrightnessOverlay.ShowOverlay(brightness);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isCapturingHotkey)
            return;

        // Get the actual key pressed
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore modifier-only presses - keep capturing
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            e.Handled = true;
            return; // Keep _isCapturingHotkey = true, wait for actual key
        }

        e.Handled = true;
        _isCapturingHotkey = false;

        // Build modifiers manually to include Win key
        var modifiers = System.Windows.Input.ModifierKeys.None;
        
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            modifiers |= System.Windows.Input.ModifierKeys.Control;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            modifiers |= System.Windows.Input.ModifierKeys.Alt;
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            modifiers |= System.Windows.Input.ModifierKeys.Shift;
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            modifiers |= System.Windows.Input.ModifierKeys.Windows;

        ViewModel.SetNewHotkey(modifiers, key);
        RegisterCurrentHotkey();
    }

    private void RefreshRateButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Models.RefreshRateOption option)
        {
            ViewModel.SetRefreshRate(option.Rate);
        }
    }

    private void CycleRateButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Models.RefreshRateOption option)
        {
            option.IsIncludedInCycle = !option.IsIncludedInCycle;
            ViewModel.SaveCycleRates();
        }
    }

    private bool _isSliderDragging;

    private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Update brightness whenever slider value changes (click, drag, or programmatic)
        if (sender is System.Windows.Controls.Slider slider)
        {
            ViewModel.SetBrightness((int)slider.Value);
        }
    }

    private void BrightnessSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Slider slider)
        {
            _isSliderDragging = true;
            slider.CaptureMouse();
            
            // Immediately set value based on click position
            UpdateSliderValueFromMouse(slider, e);
            
            e.Handled = true; // Prevent default slider behavior
        }
    }

    private void BrightnessSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Slider slider && _isSliderDragging)
        {
            _isSliderDragging = false;
            slider.ReleaseMouseCapture();
        }
    }

    private void BrightnessSlider_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is System.Windows.Controls.Slider slider && _isSliderDragging)
        {
            UpdateSliderValueFromMouse(slider, e);
        }
    }

    private void UpdateSliderValueFromMouse(System.Windows.Controls.Slider slider, System.Windows.Input.MouseEventArgs e)
    {
        var mousePos = e.GetPosition(slider);
        double ratio = mousePos.X / slider.ActualWidth;
        ratio = Math.Max(0, Math.Min(1, ratio)); // Clamp to 0-1
        double newValue = slider.Minimum + (ratio * (slider.Maximum - slider.Minimum));
        slider.Value = newValue;
    }

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        // Reduce scroll speed by dividing the delta
        var scrollViewer = sender as System.Windows.Controls.ScrollViewer;
        if (scrollViewer != null)
        {
            double scrollAmount = e.Delta / 3.0; // Reduce speed to 1/3
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollAmount);
            e.Handled = true;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (ViewModel.MinimizeToTrayOnClose)
        {
            // Cancel the close and minimize to tray instead
            e.Cancel = true;
            Hide();
        }
        else
        {
            // Actually close the application
            _hotkeyService.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
        
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _hotkeyService.Dispose();
        base.OnClosed(e);
    }
}