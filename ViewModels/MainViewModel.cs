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
    private HotkeyBinding _brightnessUpHotkey;
    private HotkeyBinding _brightnessDownHotkey;
    private bool _isCapturingHotkey;
    private string _hotkeyDisplayText = string.Empty;
    private string _brightnessUpHotkeyDisplayText = string.Empty;
    private string _brightnessDownHotkeyDisplayText = string.Empty;
    private bool _isDarkMode;
    private string _capturingHotkeyType = string.Empty;
    private bool _minimizeToTrayOnClose = true;
    private bool _runAtStartup;
    private bool _isRefreshRateHotkeyEnabled = true;
    private bool _isBrightnessHotkeysEnabled = true;
    private int _currentBrightness;
    private bool _isBrightnessSupported;

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

    public HotkeyBinding BrightnessUpHotkey
    {
        get => _brightnessUpHotkey;
        set
        {
            if (SetProperty(ref _brightnessUpHotkey, value))
            {
                BrightnessUpHotkeyDisplayText = value?.ToString() ?? "Not set";
            }
        }
    }

    public string BrightnessUpHotkeyDisplayText
    {
        get => _brightnessUpHotkeyDisplayText;
        set => SetProperty(ref _brightnessUpHotkeyDisplayText, value);
    }

    public HotkeyBinding BrightnessDownHotkey
    {
        get => _brightnessDownHotkey;
        set
        {
            if (SetProperty(ref _brightnessDownHotkey, value))
            {
                BrightnessDownHotkeyDisplayText = value?.ToString() ?? "Not set";
            }
        }
    }

    public string BrightnessDownHotkeyDisplayText
    {
        get => _brightnessDownHotkeyDisplayText;
        set => SetProperty(ref _brightnessDownHotkeyDisplayText, value);
    }

    public bool IsCapturingHotkey
    {
        get => _isCapturingHotkey;
        set => SetProperty(ref _isCapturingHotkey, value);
    }

    public string CapturingHotkeyType
    {
        get => _capturingHotkeyType;
        set => SetProperty(ref _capturingHotkeyType, value);
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

    public bool RunAtStartup
    {
        get => _runAtStartup;
        set
        {
            if (SetProperty(ref _runAtStartup, value))
            {
                Helpers.StartupManager.IsEnabled = value;
                SaveSettings();
            }
        }
    }

    public bool IsRefreshRateHotkeyEnabled
    {
        get => _isRefreshRateHotkeyEnabled;
        set
        {
            if (SetProperty(ref _isRefreshRateHotkeyEnabled, value))
            {
                SaveSettings();
                RefreshRateHotkeyToggled?.Invoke();
            }
        }
    }

    public bool IsBrightnessHotkeysEnabled
    {
        get => _isBrightnessHotkeysEnabled;
        set
        {
            if (SetProperty(ref _isBrightnessHotkeysEnabled, value))
            {
                SaveSettings();
                BrightnessHotkeysToggled?.Invoke();
            }
        }
    }

    public int CurrentBrightness
    {
        get => _currentBrightness;
        set => SetProperty(ref _currentBrightness, value);
    }

    public bool IsBrightnessSupported
    {
        get => _isBrightnessSupported;
        private set => SetProperty(ref _isBrightnessSupported, value);
    }

    public ICommand CycleRefreshRateCommand { get; }
    public ICommand RefreshDisplayInfoCommand { get; }
    public ICommand StartCaptureHotkeyCommand { get; }
    public ICommand StartCaptureBrightnessUpHotkeyCommand { get; }
    public ICommand StartCaptureBrightnessDownHotkeyCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand CancelHotkeyCaptureCommand { get; }
    public ICommand IncreaseBrightnessCommand { get; }
    public ICommand DecreaseBrightnessCommand { get; }

    // Events for the view to handle
    public event Action? RequestHotkeyCapture;
    public event Action<int>? RefreshRateChanged;
    public event Action<int>? BrightnessChanged;
    public event Action? RefreshRateHotkeyToggled;
    public event Action? BrightnessHotkeysToggled;

    public MainViewModel()
    {
        _themeService = new ThemeService();
        _settings = SettingsService.Load();

        // Apply loaded settings
        _isDarkMode = _settings.IsDarkMode;
        _minimizeToTrayOnClose = _settings.MinimizeToTrayOnClose;
        _runAtStartup = _settings.RunAtStartup;
        _isRefreshRateHotkeyEnabled = _settings.IsRefreshRateHotkeyEnabled;
        _isBrightnessHotkeysEnabled = _settings.IsBrightnessHotkeysEnabled;
        _themeService.CurrentTheme = _isDarkMode ? ThemeService.Theme.Dark : ThemeService.Theme.Light;
        
        // Sync registry with saved setting
        Helpers.StartupManager.IsEnabled = _runAtStartup;

        // Initialize hotkey from settings
        _cycleRefreshRateHotkey = new HotkeyBinding(
            "Cycle Refresh Rate",
            "CycleRefreshRate",
            _settings.GetModifierKeys(),
            _settings.GetKey(),
            1);
        _hotkeyDisplayText = _cycleRefreshRateHotkey.ToString();

        // Initialize brightness hotkeys from settings
        _brightnessUpHotkey = new HotkeyBinding(
            "Brightness Up",
            "BrightnessUp",
            _settings.GetBrightnessUpModifiers(),
            _settings.GetBrightnessUpKey(),
            2);
        _brightnessUpHotkeyDisplayText = _brightnessUpHotkey.ToString();

        _brightnessDownHotkey = new HotkeyBinding(
            "Brightness Down",
            "BrightnessDown",
            _settings.GetBrightnessDownModifiers(),
            _settings.GetBrightnessDownKey(),
            3);
        _brightnessDownHotkeyDisplayText = _brightnessDownHotkey.ToString();

        // Commands
        CycleRefreshRateCommand = new RelayCommand(_ => ExecuteCycleRefreshRate());
        RefreshDisplayInfoCommand = new RelayCommand(_ => RefreshDisplayInfo());
        StartCaptureHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture("CycleRefreshRate"), _ => !IsCapturingHotkey);
        StartCaptureBrightnessUpHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture("BrightnessUp"), _ => !IsCapturingHotkey);
        StartCaptureBrightnessDownHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture("BrightnessDown"), _ => !IsCapturingHotkey);
        ToggleThemeCommand = new RelayCommand(_ => IsDarkMode = !IsDarkMode);
        CancelHotkeyCaptureCommand = new RelayCommand(_ => CancelHotkeyCapture());
        IncreaseBrightnessCommand = new RelayCommand(_ => ExecuteIncreaseBrightness(), _ => IsBrightnessSupported);
        DecreaseBrightnessCommand = new RelayCommand(_ => ExecuteDecreaseBrightness(), _ => IsBrightnessSupported);

        // Load initial display info
        RefreshDisplayInfo();
        
        // Initialize brightness
        InitializeBrightness();
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

    private void StartHotkeyCapture(string hotkeyType)
    {
        IsCapturingHotkey = true;
        CapturingHotkeyType = hotkeyType;
        
        switch (hotkeyType)
        {
            case "CycleRefreshRate":
                HotkeyDisplayText = "Press keys...";
                break;
            case "BrightnessUp":
                BrightnessUpHotkeyDisplayText = "Press keys...";
                break;
            case "BrightnessDown":
                BrightnessDownHotkeyDisplayText = "Press keys...";
                break;
        }
        
        RequestHotkeyCapture?.Invoke();
    }

    public void CancelHotkeyCapture()
    {
        IsCapturingHotkey = false;
        
        // Restore the original display text based on what was being captured
        switch (CapturingHotkeyType)
        {
            case "CycleRefreshRate":
                HotkeyDisplayText = CycleRefreshRateHotkey.ToString();
                break;
            case "BrightnessUp":
                BrightnessUpHotkeyDisplayText = BrightnessUpHotkey.ToString();
                break;
            case "BrightnessDown":
                BrightnessDownHotkeyDisplayText = BrightnessDownHotkey.ToString();
                break;
        }
        
        CapturingHotkeyType = string.Empty;
        StatusMessage = "Hotkey capture cancelled";
    }

    public void SetNewHotkey(ModifierKeys modifiers, Key key)
    {
        IsCapturingHotkey = false;
        
        if (key == Key.Escape)
        {
            CancelHotkeyCapture();
            return;
        }

        if (key != Key.None && modifiers != ModifierKeys.None)
        {
            switch (CapturingHotkeyType)
            {
                case "CycleRefreshRate":
                    CycleRefreshRateHotkey = new HotkeyBinding("Cycle Refresh Rate", "CycleRefreshRate", modifiers, key, 1);
                    StatusMessage = $"Refresh rate hotkey set to {CycleRefreshRateHotkey}";
                    break;
                case "BrightnessUp":
                    BrightnessUpHotkey = new HotkeyBinding("Brightness Up", "BrightnessUp", modifiers, key, 2);
                    StatusMessage = $"Brightness up hotkey set to {BrightnessUpHotkey}";
                    break;
                case "BrightnessDown":
                    BrightnessDownHotkey = new HotkeyBinding("Brightness Down", "BrightnessDown", modifiers, key, 3);
                    StatusMessage = $"Brightness down hotkey set to {BrightnessDownHotkey}";
                    break;
            }
            SaveSettings();
        }
        else
        {
            // Restore the original text
            switch (CapturingHotkeyType)
            {
                case "CycleRefreshRate":
                    HotkeyDisplayText = CycleRefreshRateHotkey.ToString();
                    break;
                case "BrightnessUp":
                    BrightnessUpHotkeyDisplayText = BrightnessUpHotkey.ToString();
                    break;
                case "BrightnessDown":
                    BrightnessDownHotkeyDisplayText = BrightnessDownHotkey.ToString();
                    break;
            }
            StatusMessage = "Invalid hotkey (need modifier + key)";
        }
        
        CapturingHotkeyType = string.Empty;
    }

    /// <summary>
    /// Saves the current cycle rate selection to settings.
    /// Called when cycle rates are toggled.
    /// </summary>
    public void SaveCycleRates()
    {
        SaveSettings();
    }

    private void InitializeBrightness()
    {
        int brightness = BrightnessService.GetBrightness();
        IsBrightnessSupported = brightness >= 0;
        
        if (IsBrightnessSupported)
        {
            _currentBrightness = brightness;
            OnPropertyChanged(nameof(CurrentBrightness));
        }
    }

    public void ExecuteIncreaseBrightness()
    {
        int newBrightness = BrightnessService.IncreaseBrightness(10);
        if (newBrightness >= 0)
        {
            _currentBrightness = newBrightness;
            OnPropertyChanged(nameof(CurrentBrightness));
            StatusMessage = $"Brightness: {newBrightness}%";
            BrightnessChanged?.Invoke(newBrightness);
        }
    }

    public void ExecuteDecreaseBrightness()
    {
        int newBrightness = BrightnessService.DecreaseBrightness(10);
        if (newBrightness >= 0)
        {
            _currentBrightness = newBrightness;
            OnPropertyChanged(nameof(CurrentBrightness));
            StatusMessage = $"Brightness: {newBrightness}%";
            BrightnessChanged?.Invoke(newBrightness);
        }
    }

    /// <summary>
    /// Sets brightness to a specific value (called from slider).
    /// </summary>
    public void SetBrightness(int brightness)
    {
        brightness = Math.Clamp(brightness, 0, 100);
        if (BrightnessService.SetBrightness(brightness))
        {
            _currentBrightness = brightness;
            OnPropertyChanged(nameof(CurrentBrightness));
            StatusMessage = $"Brightness: {brightness}%";
            BrightnessChanged?.Invoke(brightness);
        }
    }

    private void SaveSettings()
    {
        _settings.IsDarkMode = IsDarkMode;
        _settings.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
        _settings.RunAtStartup = RunAtStartup;
        _settings.HotkeyModifiers = CycleRefreshRateHotkey.Modifiers.ToString();
        _settings.HotkeyKey = CycleRefreshRateHotkey.Key.ToString();
        
        // Save brightness hotkeys
        _settings.BrightnessUpModifiers = BrightnessUpHotkey.Modifiers.ToString();
        _settings.BrightnessUpKey = BrightnessUpHotkey.Key.ToString();
        _settings.BrightnessDownModifiers = BrightnessDownHotkey.Modifiers.ToString();
        _settings.BrightnessDownKey = BrightnessDownHotkey.Key.ToString();
        
        // Save hotkey enabled states
        _settings.IsRefreshRateHotkeyEnabled = IsRefreshRateHotkeyEnabled;
        _settings.IsBrightnessHotkeysEnabled = IsBrightnessHotkeysEnabled;
        
        // Save cycle rates
        var cycleRates = AvailableRefreshRates
            .Where(r => r.IsIncludedInCycle)
            .Select(r => r.Rate);
        _settings.SetCycleRefreshRates(cycleRates);
        
        SettingsService.Save(_settings);
    }
}
