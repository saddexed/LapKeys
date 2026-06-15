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
    private bool _useUnifiedHotkeys;
    private int _currentBrightness;
    private int _targetBrightness = -1;
    private DateTime _lastBrightnessSetTime = DateTime.MinValue;
    private bool _isBrightnessSupported;
    private bool _isExternalBrightnessUpdate;
    private MonitorInfo? _selectedMonitor;
    private string? _internalDisplayDeviceName;

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

    public ObservableCollection<MonitorInfo> Monitors { get; } = new();

    public MonitorInfo? SelectedMonitor
    {
        get => _selectedMonitor;
        set
        {
            if (SetProperty(ref _selectedMonitor, value))
            {
                UpdateMonitorSelection();
                RefreshDisplayInfo();
                if (_selectedMonitor != null)
                {
                    StatusMessage = $"Selected {DescribeMonitor(_selectedMonitor.DeviceName)}";
                }
            }
        }
    }

    private void UpdateMonitorSelection()
    {
        foreach (var monitor in Monitors)
        {
            monitor.IsSelected = (monitor == _selectedMonitor);
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value) && !string.IsNullOrWhiteSpace(value))
            {
                LogService.Info(value);
            }
        }
    }

    private System.Windows.Media.Brush _statusBrush = StatusInfoBrush;
    public System.Windows.Media.Brush StatusBrush
    {
        get => _statusBrush;
        set => SetProperty(ref _statusBrush, value);
    }

    private static readonly System.Windows.Media.Brush StatusInfoBrush = MakeFrozen(0x88, 0x88, 0x88);
    private static readonly System.Windows.Media.Brush StatusWarnBrush = MakeFrozen(0xE0, 0xA0, 0x30);
    private static readonly System.Windows.Media.Brush StatusErrorBrush = MakeFrozen(0xE0, 0x50, 0x50);

    private static System.Windows.Media.Brush MakeFrozen(byte r, byte g, byte b)
    {
        var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    private void OnLogged(LogLevel level, string message)
    {
        // LogService may fire from background threads; marshal to the UI thread.
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(new Action(() => OnLogged(level, message)));
            return;
        }

        StatusBrush = level switch
        {
            LogLevel.Error => StatusErrorBrush,
            LogLevel.Warning => StatusWarnBrush,
            _ => StatusInfoBrush
        };

        // Surface warnings/errors that did not originate from a StatusMessage assignment.
        if (level != LogLevel.Info)
        {
            SetProperty(ref _statusMessage, message, nameof(StatusMessage));
        }
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

    public bool UseUnifiedHotkeys
    {
        get => _useUnifiedHotkeys;
        set
        {
            if (SetProperty(ref _useUnifiedHotkeys, value))
            {
                _settings.UseUnifiedHotkeys = value;

                // Load the hotkeys belonging to the mode we just switched into, so the UI
                // and registrations reflect that profile (and we don't overwrite the other
                // mode's stored slots).
                LoadBrightnessHotkeys();
                RefreshDisplayInfo();

                SettingsService.Save(_settings);

                // Re-register both refresh-rate and brightness hotkeys for the new mode.
                RefreshRateHotkeyToggled?.Invoke();
                BrightnessHotkeysToggled?.Invoke();
            }
        }
    }

    public int CurrentBrightness
    {
        get => _currentBrightness;
        set
        {
            int clamped = Math.Clamp(value, 0, 100);
            if (clamped == _currentBrightness)
                return;

            // External/WMI updates set the field directly; only slider/user edits
            // reach this setter, so apply them to the hardware.
            if (_isExternalBrightnessUpdate)
            {
                SetProperty(ref _currentBrightness, clamped);
            }
            else
            {
                SetBrightness(clamped);
            }
        }
    }

    public bool IsBrightnessSupported
    {
        get => _isBrightnessSupported;
        private set => SetProperty(ref _isBrightnessSupported, value);
    }

    public bool IsExternalBrightnessUpdate => _isExternalBrightnessUpdate;

    public bool HasMultipleMonitors => Monitors.Count > 1;

    public ICommand CycleRefreshRateCommand { get; }
    public ICommand RefreshDisplayInfoCommand { get; }
    public ICommand StartCaptureHotkeyCommand { get; }
    public ICommand StartCaptureBrightnessUpHotkeyCommand { get; }
    public ICommand StartCaptureBrightnessDownHotkeyCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleCaptureHotkeyCommand { get; }
    public ICommand CancelHotkeyCaptureCommand { get; }
    public ICommand IncreaseBrightnessCommand { get; }
    public ICommand DecreaseBrightnessCommand { get; }
    public ICommand RefreshMonitorsCommand { get; }
    public ICommand SelectMonitorCommand { get; }

    public event Action? RequestHotkeyCapture;
    public event Action<int, string?>? RefreshRateChanged;

    public List<HotkeyBinding> GetAllRefreshRateHotkeys()
    {
        var hotkeys = new List<HotkeyBinding>();
        // Unified mode: a single hotkey that acts on whichever monitor is in view.
        // Empty DeviceName is the sentinel for "resolve the active monitor at press time".
        if (_useUnifiedHotkeys)
        {
            var unified = new HotkeyBinding(
                "Cycle Refresh Rate",
                "CycleRefreshRate",
                _settings.GetUnifiedModifiers(),
                _settings.GetUnifiedKey(),
                10) { DeviceName = "" };

            if (unified.Key != Key.None)
            {
                hotkeys.Add(unified);
            }
            return hotkeys;
        }

        foreach (var monitor in Monitors)
        {
            var binding = new HotkeyBinding(
                "Cycle Refresh Rate",
                "CycleRefreshRate",
                _settings.GetModifierKeys(monitor.DeviceName),
                _settings.GetKey(monitor.DeviceName),
                hotkeys.Count + 10) { DeviceName = monitor.DeviceName };

            if (binding.Key != Key.None)
            {
                hotkeys.Add(binding);
            }
        }

        // Add fallback if list is empty, e.g. for primary monitor
        if (hotkeys.Count == 0 && CycleRefreshRateHotkey != null && CycleRefreshRateHotkey.Key != Key.None)
        {
            hotkeys.Add(CycleRefreshRateHotkey);
        }

        return hotkeys;
    }

    /// <summary>
    /// Loads the brightness up/down hotkeys for the current mode (unified profile vs normal),
    /// reading from the matching config slots so toggling modes restores each profile.
    /// </summary>
    private void LoadBrightnessHotkeys()
    {
        if (_useUnifiedHotkeys)
        {
            BrightnessUpHotkey = new HotkeyBinding("Brightness Up", "BrightnessUp",
                _settings.GetUnifiedBrightnessUpModifiers(), _settings.GetUnifiedBrightnessUpKey(), 2);
            BrightnessDownHotkey = new HotkeyBinding("Brightness Down", "BrightnessDown",
                _settings.GetUnifiedBrightnessDownModifiers(), _settings.GetUnifiedBrightnessDownKey(), 3);
        }
        else
        {
            BrightnessUpHotkey = new HotkeyBinding("Brightness Up", "BrightnessUp",
                _settings.GetBrightnessUpModifiers(), _settings.GetBrightnessUpKey(), 2);
            BrightnessDownHotkey = new HotkeyBinding("Brightness Down", "BrightnessDown",
                _settings.GetBrightnessDownModifiers(), _settings.GetBrightnessDownKey(), 3);
        }

        BrightnessUpHotkeyDisplayText = BrightnessUpHotkey.ToString();
        BrightnessDownHotkeyDisplayText = BrightnessDownHotkey.ToString();
    }

    public event Action<int, string?>? BrightnessChanged;
    public event Action? RefreshRateHotkeyToggled;
    public event Action? BrightnessHotkeysToggled;
    public MainViewModel()
    {
        _themeService = new ThemeService();
        _settings = SettingsService.Load();

        _isDarkMode = _settings.IsDarkMode;
        _minimizeToTrayOnClose = _settings.MinimizeToTrayOnClose;
        _runAtStartup = _settings.RunAtStartup;
        _isRefreshRateHotkeyEnabled = _settings.IsRefreshRateHotkeyEnabled;
        _isBrightnessHotkeysEnabled = _settings.IsBrightnessHotkeysEnabled;
        _useUnifiedHotkeys = _settings.UseUnifiedHotkeys;
        _themeService.CurrentTheme = _isDarkMode ? ThemeService.Theme.Dark : ThemeService.Theme.Light;
        _themeService.Initialize();
        
        Helpers.StartupManager.IsEnabled = _runAtStartup;

        // Initialize with default or empty
        _cycleRefreshRateHotkey = new HotkeyBinding(
            "Cycle Refresh Rate",
            "CycleRefreshRate",
            ModifierKeys.None,
            Key.None,
            1);

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

        LoadBrightnessHotkeys();

        LogService.Logged += OnLogged;

        CycleRefreshRateCommand = new RelayCommand(_ => ExecuteCycleRefreshRate());
        RefreshDisplayInfoCommand = new RelayCommand(_ => RefreshDisplayInfo());
        StartCaptureHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture("CycleRefreshRate"), _ => !IsCapturingHotkey);
        StartCaptureBrightnessUpHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture("BrightnessUp"), _ => !IsCapturingHotkey);
        StartCaptureBrightnessDownHotkeyCommand = new RelayCommand(_ => StartHotkeyCapture("BrightnessDown"), _ => !IsCapturingHotkey);
        ToggleThemeCommand = new RelayCommand(_ => IsDarkMode = !IsDarkMode);
        ToggleCaptureHotkeyCommand = new RelayCommand(param => ToggleCaptureHotkey(param as string ?? string.Empty));
        CancelHotkeyCaptureCommand = new RelayCommand(_ => CancelHotkeyCapture());
        IncreaseBrightnessCommand = new RelayCommand(_ => ExecuteIncreaseBrightness(), _ => IsBrightnessSupported);
        DecreaseBrightnessCommand = new RelayCommand(_ => ExecuteDecreaseBrightness(), _ => IsBrightnessSupported);
        RefreshMonitorsCommand = new RelayCommand(_ => InitializeMonitors());
        SelectMonitorCommand = new RelayCommand(monitor => 
        {
            if (monitor is MonitorInfo m)
            {
                SelectedMonitor = m;
            }
        });

        InitializeMonitors();
        RefreshDisplayInfo();
        InitializeBrightness();
    }

    private void InitializeMonitors()
    {
        var monitors = DisplayService.GetMonitors();
        Monitors.Clear();
        foreach (var monitor in monitors)
        {
            Monitors.Add(monitor);
        }
        
        if (Monitors.Count > 0)
        {
            SelectedMonitor = Monitors[0];
            OnPropertyChanged(nameof(HasMultipleMonitors));
        }
    }

    public void RefreshDisplayInfo()
    {
        string? deviceName = SelectedMonitor?.DeviceName;
        var currentMode = DisplayService.GetCurrentDisplayMode(deviceName);
        if (currentMode != null)
        {
            CurrentRefreshRate = currentMode.RefreshRate;
            Title = $"LapKeys - {currentMode.Width}x{currentMode.Height}@{currentMode.RefreshRate}Hz";
        }

        var savedCycleRates = _settings.GetCycleRefreshRates(deviceName);

        AvailableRefreshRates.Clear();
        foreach (var rate in DisplayService.GetAvailableRefreshRates(deviceName))
        {
            bool isIncludedInCycle = savedCycleRates.Count == 0 || savedCycleRates.Contains(rate);
            AvailableRefreshRates.Add(new RefreshRateOption(rate, rate == CurrentRefreshRate, isIncludedInCycle));
        }

        if (_useUnifiedHotkeys)
        {
            // One global hotkey shared across all monitors (unified profile slot).
            CycleRefreshRateHotkey = new HotkeyBinding(
                "Cycle Refresh Rate",
                "CycleRefreshRate",
                _settings.GetUnifiedModifiers(),
                _settings.GetUnifiedKey(),
                1) { DeviceName = "" };
        }
        else if (deviceName != null)
        {
            CycleRefreshRateHotkey = new HotkeyBinding(
                "Cycle Refresh Rate",
                "CycleRefreshRate",
                _settings.GetModifierKeys(deviceName),
                _settings.GetKey(deviceName),
                1) { DeviceName = deviceName };
        }
        
        // WMI Brightness generally only supports the built-in laptop display (usually the primary one).
        // Since we don't have DDC/CI yet, we assume the first/primary monitor is the only one with WMI brightness.
        // A more robust check could match WMI InstanceName with DeviceID.
        bool isPrimary = deviceName == null || deviceName == Monitors.FirstOrDefault()?.DeviceName;
        IsBrightnessSupported = isPrimary && BrightnessService.GetBrightness() >= 0;
    }

    private void UpdateRefreshRateSelection()
    {
        foreach (var option in AvailableRefreshRates)
        {
            option.IsSelected = option.Rate == CurrentRefreshRate;
        }
    }

    public void ExecuteCycleRefreshRate(string? deviceName = null)
    {
        deviceName ??= SelectedMonitor?.DeviceName;
        
        var cycleRates = _settings.GetCycleRefreshRates(deviceName);
        if (cycleRates.Count == 0 && deviceName == SelectedMonitor?.DeviceName)
        {
            cycleRates = AvailableRefreshRates
                .Where(r => r.IsIncludedInCycle)
                .Select(r => r.Rate)
                .ToList();
        }

        int newRate = DisplayService.CycleRefreshRate(cycleRates, deviceName);
        if (newRate > 0)
        {
            if (deviceName == SelectedMonitor?.DeviceName)
            {
                CurrentRefreshRate = newRate;
                RefreshDisplayInfo();
            }
            StatusMessage = $"Changed {DescribeMonitor(deviceName)} refresh rate to {newRate}Hz";
            RefreshRateChanged?.Invoke(newRate, deviceName);
        }
        else
        {
            StatusMessage = $"Failed to cycle {DescribeMonitor(deviceName)} refresh rate";
        }
    }

    public void SetRefreshRate(int refreshRate)
    {
        string? deviceName = SelectedMonitor?.DeviceName;
        if (DisplayService.SetRefreshRate(refreshRate, deviceName))
        {
            CurrentRefreshRate = refreshRate;
            StatusMessage = $"Changed {DescribeMonitor(deviceName)} refresh rate to {refreshRate}Hz";
            RefreshDisplayInfo();
            RefreshRateChanged?.Invoke(refreshRate, deviceName);
        }
        else
        {
            StatusMessage = $"Failed to set {DescribeMonitor(deviceName)} refresh rate to {refreshRate}Hz";
        }
    }

    /// <summary>
    /// Returns a friendly label for a monitor device name for log/status messages,
    /// e.g. "Monitor 1 (\\.\DISPLAY1)". Falls back to "Primary" when unknown.
    /// </summary>
    private string DescribeMonitor(string? deviceName)
    {
        if (string.IsNullOrEmpty(deviceName))
            return "Primary";

        var monitor = Monitors.FirstOrDefault(m => m.DeviceName == deviceName);
        return monitor != null ? $"Monitor {monitor.Index} ({deviceName})" : deviceName;
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

    private void ToggleCaptureHotkey(string hotkeyType)
    {
        if (IsCapturingHotkey)
        {
            CancelHotkeyCapture();
        }
        else
        {
            StartHotkeyCapture(hotkeyType);
        }
    }

    public void CancelHotkeyCapture()
    {
        IsCapturingHotkey = false;
        
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
                    CycleRefreshRateHotkey = new HotkeyBinding("Cycle Refresh Rate", "CycleRefreshRate", modifiers, key, 1)
                        { DeviceName = _useUnifiedHotkeys ? "" : (SelectedMonitor?.DeviceName ?? "") };
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

    public void SaveSettingsPublic()
    {
        SaveSettings();
    }

    public void SaveCycleRates()
    {
        SaveSettings();
    }

    private void InitializeBrightness()
    {
        _internalDisplayDeviceName = DisplayService.GetInternalDisplayDeviceName();

        int brightness = BrightnessService.GetBrightness();
        IsBrightnessSupported = brightness >= 0;
        
        if (IsBrightnessSupported)
        {
            _currentBrightness = brightness;
            OnPropertyChanged(nameof(CurrentBrightness));
            
            BrightnessService.BrightnessChanged += OnExternalBrightnessChanged;
            BrightnessService.StartWatching();
        }
    }

    private void OnExternalBrightnessChanged(int brightness)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            // Ignore WMI events that arrive shortly after we set the brightness
            // as WMI on battery might report scaled physical brightness instead of logical
            if ((DateTime.UtcNow - _lastBrightnessSetTime).TotalSeconds < 2)
            {
                return;
            }

            _isExternalBrightnessUpdate = true;
            _targetBrightness = brightness;
            _currentBrightness = brightness;
            OnPropertyChanged(nameof(CurrentBrightness));
            _isExternalBrightnessUpdate = false;
        });
    }

    public void ExecuteIncreaseBrightness()
    {
        int baseBrightness = _targetBrightness >= 0 ? _targetBrightness : _currentBrightness;
        int target = Math.Min(100, baseBrightness + 10);
        SetBrightness(target);
    }

    public void ExecuteDecreaseBrightness()
    {
        int baseBrightness = _targetBrightness >= 0 ? _targetBrightness : _currentBrightness;
        int target = Math.Max(0, baseBrightness - 10);
        SetBrightness(target);
    }

    public void SetBrightness(int brightness)
    {
        brightness = Math.Clamp(brightness, 0, 100);
        _lastBrightnessSetTime = DateTime.UtcNow;
        if (BrightnessService.SetBrightness(brightness))
        {
            _targetBrightness = brightness;
            _currentBrightness = brightness;
            OnPropertyChanged(nameof(CurrentBrightness));
            StatusMessage = $"Changed {DescribeMonitor(_internalDisplayDeviceName)} brightness to {brightness}%";
            BrightnessChanged?.Invoke(brightness, _internalDisplayDeviceName);
        }
    }

    private void SaveSettings()
    {
        _settings.IsDarkMode = IsDarkMode;
        _settings.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
        _settings.RunAtStartup = RunAtStartup;
        
        string? deviceName = SelectedMonitor?.DeviceName;
        if (CycleRefreshRateHotkey != null)
        {
            if (_useUnifiedHotkeys)
            {
                // Persist in the unified profile slot, separate from per-monitor hotkeys.
                _settings.UnifiedHotkeyModifiers = CycleRefreshRateHotkey.Modifiers.ToString();
                _settings.UnifiedHotkeyKey = CycleRefreshRateHotkey.Key.ToString();
            }
            else if (deviceName != null)
            {
                _settings.MonitorHotkeyModifiers[deviceName] = CycleRefreshRateHotkey.Modifiers.ToString();
                _settings.MonitorHotkeyKeys[deviceName] = CycleRefreshRateHotkey.Key.ToString();
            }
        }

        if (_useUnifiedHotkeys)
        {
            // Brightness hotkeys belong to the unified profile slot.
            _settings.UnifiedBrightnessUpModifiers = BrightnessUpHotkey.Modifiers.ToString();
            _settings.UnifiedBrightnessUpKey = BrightnessUpHotkey.Key.ToString();
            _settings.UnifiedBrightnessDownModifiers = BrightnessDownHotkey.Modifiers.ToString();
            _settings.UnifiedBrightnessDownKey = BrightnessDownHotkey.Key.ToString();
        }
        else
        {
            _settings.BrightnessUpModifiers = BrightnessUpHotkey.Modifiers.ToString();
            _settings.BrightnessUpKey = BrightnessUpHotkey.Key.ToString();
            _settings.BrightnessDownModifiers = BrightnessDownHotkey.Modifiers.ToString();
            _settings.BrightnessDownKey = BrightnessDownHotkey.Key.ToString();
        }

        _settings.IsRefreshRateHotkeyEnabled = IsRefreshRateHotkeyEnabled;
        _settings.IsBrightnessHotkeysEnabled = IsBrightnessHotkeysEnabled;
        _settings.UseUnifiedHotkeys = UseUnifiedHotkeys;
        
        var cycleRates = AvailableRefreshRates
            .Where(r => r.IsIncludedInCycle)
            .Select(r => r.Rate);
        _settings.SetCycleRefreshRates(cycleRates, deviceName);
        
        SettingsService.Save(_settings);
    }
}
