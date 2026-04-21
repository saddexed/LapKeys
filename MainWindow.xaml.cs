using System.Windows;
using LapKeys.Services;
using LapKeys.ViewModels;
using LapKeys.Views;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;
using Keyboard = System.Windows.Input.Keyboard;
using Button = System.Windows.Controls.Button;

namespace LapKeys;

public partial class MainWindow : Window
{
    private readonly HotkeyService _hotkeyService;
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private bool _isCapturingHotkey;

    public MainWindow()
    {
        InitializeComponent();
        
        // Force the creation of the handle so hotkeys can be registered
        // even if the window is never shown (e.g., --minimized)
        var helper = new System.Windows.Interop.WindowInteropHelper(this);
        helper.EnsureHandle();

        DataContext = new MainViewModel();
        _hotkeyService = new HotkeyService();
        
        ViewModel.RequestHotkeyCapture += OnRequestHotkeyCapture;
        ViewModel.RefreshRateChanged += OnRefreshRateChanged;
        ViewModel.BrightnessChanged += OnBrightnessChanged;
        ViewModel.RefreshRateHotkeyToggled += RegisterCurrentHotkey;
        ViewModel.BrightnessHotkeysToggled += RegisterCurrentHotkey;
        
        _hotkeyService.Initialize(this);
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        
        RegisterCurrentHotkey();
    }

    private void RegisterCurrentHotkey()
    {
        _hotkeyService.UnregisterAllHotkeys();
        
        if (ViewModel.IsRefreshRateHotkeyEnabled)
        {
            var hotkeys = ViewModel.GetAllRefreshRateHotkeys();
            foreach (var hk in hotkeys)
            {
                if (_hotkeyService.RegisterHotkey(hk))
                {
                    ViewModel.StatusMessage = $"Hotkey registered: {hk}";
                }
                else
                {
                    ViewModel.StatusMessage = $"Failed to register hotkey (may be in use by another app)";
                }
            }
        }
        
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
                ViewModel.ExecuteCycleRefreshRate(binding.DeviceName);
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
        _hotkeyService.UnregisterAllHotkeys();
        Focus();
    }

    private void OnRefreshRateChanged(int newRate, string? deviceName)
    {
        RefreshRateOverlay.ShowOverlay(newRate, deviceName);
    }

    private void OnBrightnessChanged(int brightness)
    {
        BrightnessOverlay.ShowOverlay(brightness);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isCapturingHotkey)
            return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            e.Handled = true;
            return;
        }

        e.Handled = true;
        _isCapturingHotkey = false;

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
        if (sender is Button button && button.DataContext is LapKeys.Models.RefreshRateOption option && DataContext is MainViewModel vm)
        {
            vm.SetRefreshRate(option.Rate);
        }
    }

    private void CycleRateButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is LapKeys.Models.RefreshRateOption option && DataContext is MainViewModel vm)
        {
            option.IsIncludedInCycle = !option.IsIncludedInCycle;
            vm.SaveCycleRates();
        }
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
        var scrollViewer = sender as System.Windows.Controls.ScrollViewer;
        if (scrollViewer != null)
        {
            double scrollAmount = e.Delta / 3.0;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollAmount);
            e.Handled = true;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (ViewModel.MinimizeToTrayOnClose)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
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