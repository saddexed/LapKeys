using System.Windows;
using LapKeys.Services;
using LapKeys.ViewModels;
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
        
        if (_hotkeyService.RegisterHotkey(ViewModel.CycleRefreshRateHotkey))
        {
            ViewModel.StatusMessage = $"Hotkey registered: {ViewModel.CycleRefreshRateHotkey}";
        }
        else
        {
            ViewModel.StatusMessage = $"Failed to register hotkey (may be in use by another app)";
        }
    }

    private void OnHotkeyPressed(object? sender, Models.HotkeyBinding binding)
    {
        if (binding.Action == "CycleRefreshRate")
        {
            ViewModel.ExecuteCycleRefreshRate();
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
        // Could show a notification here
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

    protected override void OnClosed(EventArgs e)
    {
        _hotkeyService.Dispose();
        base.OnClosed(e);
    }
}