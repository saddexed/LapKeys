using System.Collections.ObjectModel;
using System.Windows.Input;
using LapKeys.Helpers;
using LapKeys.Models;
using LapKeys.Services;

namespace LapKeys.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ThemeService _themeService;
    private AppSettings _settings;
    private string _title = "LapKeys - Laptop Control";
    private int _currentRefreshRate;
    private string _statusMessage = string.Empty;
    private HotkeyBinding _cycleRefreshRateHotkey;
    private bool _isCapturingHotkey;
    private string _hotkeyDisplayText = string.Empty;
    private bool _isDarkMode;
    private bool _minimizeToTrayOnClose = true;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public int CurrentRefreshRate
    {
        get => _currentRefreshRate;
        set
        {
            if (SetProperty(ref _currentRefreshRate, value))
            {
                UpdateRefreshRateSelection();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                _themeService.CurrentTheme = value ? ThemeService.Theme.Dark : ThemeService.Theme.Light;
                SaveSettings();
            }
        }
    }

    public ObservableCollection<RefreshRateOption> AvailableRefreshRates { get; } = new();

    public HotkeyBinding CycleRefreshRateHotkey
    {
        get => _cycleRefreshRateHotkey;
        set
        {
            if (SetProperty(ref _cycleRefreshRateHotkey, value))
            {
                HotkeyDisplayText = value?.ToString() ?? "Not set";
            }
        }
    }

    public string HotkeyDisplayText
    {
        get => _hotkeyDisplayText;
        set => SetProperty(ref _hotkeyDisplayText, value);
    }

    public bool IsCapturingHotkey
    {
        get => _isCapturingHotkey;
        set => SetProperty(ref _isCapturingHotkey, value);
    }

    public bool MinimizeToTrayOnClose
    {
        get => _minimizeToTrayOnClose;
        set
        {
            if (SetProperty(ref _minimizeToTrayOnClose, value))
            {
                SaveSettings();
            }
        }
    }

    public ICommand CycleRefreshRateCommand { get; }
    public ICommand RefreshDisplayInfoCommand { get; }
    public ICommand StartCaptureHotkeyCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand CancelHotkeyCaptureCommand { get; }

    // Events for the view to handle
    public event Action? RequestHotkeyCapture;
    public event Action<int>? RefreshRateChanged;

    public MainViewModel()
    {
        _themeService = new ThemeService();
        _settings = SettingsService.Load();

        // Apply loaded settings
        _isDarkMode = _settings.IsDarkMode;
        _minimizeToTrayOnClose = _settings.MinimizeToTrayOnClose;
        _themeService.CurrentTheme = _isDarkMode ? ThemeService.Theme.Dark : ThemeService.Theme.Light;

        // Initialize hotkey from settings
        _cycleRefreshRateHotkey = new HotkeyBinding(
            "Cycle Refresh Rate",
            "CycleRefreshRate",
            _settings.GetModifierKeys(),
            _settings.GetKey(),
            1);
        _hotkeyDisplayText = _cycleRefreshRateHotkey.ToString();

        // Commands
        CycleRefreshRateCommand = new RelayCommand(_ => ExecuteCycleRefreshRate());
        RefreshDisplayInfoCommand = new RelayCommand(_ => RefreshDisplayInfo());
        StartCaptureHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture(), _ => !IsCapturingHotkey);
        ToggleThemeCommand = new RelayCommand(_ => IsDarkMode = !IsDarkMode);
        CancelHotkeyCaptureCommand = new RelayCommand(_ => CancelHotkeyCapture());

        // Load initial display info
        RefreshDisplayInfo();
    }

    public void RefreshDisplayInfo()
    {
        var currentMode = DisplayService.GetCurrentDisplayMode();
        if (currentMode != null)
        {
            CurrentRefreshRate = currentMode.RefreshRate;
            Title = $"LapKeys - {currentMode.Width}x{currentMode.Height}@{currentMode.RefreshRate}Hz";
        }

        // Get saved cycle rates
        var savedCycleRates = _settings.GetCycleRefreshRates();

        // Update available refresh rates
        AvailableRefreshRates.Clear();
        foreach (var rate in DisplayService.GetAvailableRefreshRates())
        {
            // If we have saved cycle rates, use those; otherwise default to all included
            bool isIncludedInCycle = savedCycleRates.Count == 0 || savedCycleRates.Contains(rate);
            AvailableRefreshRates.Add(new RefreshRateOption(rate, rate == CurrentRefreshRate, isIncludedInCycle));
        }

        StatusMessage = $"Found {AvailableRefreshRates.Count} refresh rates";
    }

    private void UpdateRefreshRateSelection()
    {
        foreach (var option in AvailableRefreshRates)
        {
            option.IsSelected = option.Rate == CurrentRefreshRate;
        }
    }

    public void ExecuteCycleRefreshRate()
    {
        // Get the list of rates that are included in the cycle
        var cycleRates = AvailableRefreshRates
            .Where(r => r.IsIncludedInCycle)
            .Select(r => r.Rate)
            .ToList();

        int newRate = DisplayService.CycleRefreshRate(cycleRates);
        if (newRate > 0)
        {
            CurrentRefreshRate = newRate;
            StatusMessage = $"Switched to {newRate}Hz";
            RefreshDisplayInfo();
            RefreshRateChanged?.Invoke(newRate);
        }
        else
        {
            StatusMessage = "Failed to cycle refresh rate";
        }
    }

    public void SetRefreshRate(int refreshRate)
    {
        if (DisplayService.SetRefreshRate(refreshRate))
        {
            CurrentRefreshRate = refreshRate;
            StatusMessage = $"Set refresh rate to {refreshRate}Hz";
            RefreshDisplayInfo();
            RefreshRateChanged?.Invoke(refreshRate);
        }
        else
        {
            StatusMessage = $"Failed to set refresh rate to {refreshRate}Hz";
        }
    }

    private void StartHotkeyCapture()
    {
        IsCapturingHotkey = true;
        HotkeyDisplayText = "Press a key combination...";
        RequestHotkeyCapture?.Invoke();
    }

    public void CancelHotkeyCapture()
    {
        IsCapturingHotkey = false;
        HotkeyDisplayText = CycleRefreshRateHotkey.ToString();
        StatusMessage = "Hotkey capture cancelled";
    }

    public void SetNewHotkey(ModifierKeys modifiers, Key key)
    {
        IsCapturingHotkey = false;
        
        if (key == Key.Escape)
        {
            // Cancelled
            HotkeyDisplayText = CycleRefreshRateHotkey.ToString();
            StatusMessage = "Hotkey capture cancelled";
            return;
        }

        if (key != Key.None && modifiers != ModifierKeys.None)
        {
            CycleRefreshRateHotkey = new HotkeyBinding(
                "Cycle Refresh Rate",
                "CycleRefreshRate",
                modifiers,
                key,
                1);
            StatusMessage = $"Hotkey set to {CycleRefreshRateHotkey}";
            SaveSettings();
        }
        else
        {
            HotkeyDisplayText = CycleRefreshRateHotkey.ToString();
            StatusMessage = "Invalid hotkey (need modifier + key)";
        }
    }

    /// <summary>
    /// Saves the current cycle rate selection to settings.
    /// Called when cycle rates are toggled.
    /// </summary>
    public void SaveCycleRates()
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        _settings.IsDarkMode = IsDarkMode;
        _settings.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
        _settings.HotkeyModifiers = CycleRefreshRateHotkey.Modifiers.ToString();
        _settings.HotkeyKey = CycleRefreshRateHotkey.Key.ToString();
        
        // Save cycle rates
        var cycleRates = AvailableRefreshRates
            .Where(r => r.IsIncludedInCycle)
            .Select(r => r.Rate);
        _settings.SetCycleRefreshRates(cycleRates);
        
        SettingsService.Save(_settings);
    }
}
