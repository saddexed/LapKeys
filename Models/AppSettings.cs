using System.Windows.Input;

namespace LapKeys.Models;

public class AppSettings
{
    public bool IsDarkMode { get; set; } = false;
    public bool MinimizeToTrayOnClose { get; set; } = true;
    public bool RunAtStartup { get; set; } = false;
    
    public string HotkeyModifiers { get; set; } = "Control, Shift";
    public string HotkeyKey { get; set; } = "R";
    
    public string BrightnessUpModifiers { get; set; } = "Control, Shift";
    public string BrightnessUpKey { get; set; } = "Up";
    public string BrightnessDownModifiers { get; set; } = "Control, Shift";
    public string BrightnessDownKey { get; set; } = "Down";
    
    public bool IsRefreshRateHotkeyEnabled { get; set; } = true;
    public bool IsBrightnessHotkeysEnabled { get; set; } = true;
    public bool UseUnifiedHotkeys { get; set; } = false;

    // Unified-mode hotkeys. Stored separately from the per-monitor / normal hotkeys
    // so toggling UseUnifiedHotkeys off restores whatever was configured before.
    public string UnifiedHotkeyModifiers { get; set; } = "Control, Shift";
    public string UnifiedHotkeyKey { get; set; } = "R";
    public string UnifiedBrightnessUpModifiers { get; set; } = "Control, Shift";
    public string UnifiedBrightnessUpKey { get; set; } = "Up";
    public string UnifiedBrightnessDownModifiers { get; set; } = "Control, Shift";
    public string UnifiedBrightnessDownKey { get; set; } = "Down";
    
    public string CycleRefreshRates { get; set; } = "";

    public Dictionary<string, string> MonitorCycleRates { get; set; } = new();
    public Dictionary<string, string> MonitorHotkeyModifiers { get; set; } = new();
    public Dictionary<string, string> MonitorHotkeyKeys { get; set; } = new();
    
    public ModifierKeys GetModifierKeys(string? deviceName = null)
    {
        string modifiersStr = HotkeyModifiers;
        if (deviceName != null && MonitorHotkeyModifiers.TryGetValue(deviceName, out var dictVal))
        {
            modifiersStr = dictVal;
        }

        return ParseModifiers(modifiersStr);
    }
    
    public Key GetKey(string? deviceName = null)
    {
        string keyStr = HotkeyKey;
        if (deviceName != null && MonitorHotkeyKeys.TryGetValue(deviceName, out var dictVal))
        {
            keyStr = dictVal;
        }

        if (Enum.TryParse<Key>(keyStr, out var key))
            return key;
        return Key.None;
    }
    
    public ModifierKeys GetBrightnessUpModifiers()
    {
        return ParseModifiers(BrightnessUpModifiers);
    }
    
    public Key GetBrightnessUpKey()
    {
        if (Enum.TryParse<Key>(BrightnessUpKey, out var key))
            return key;
        return Key.Up;
    }
    
    public ModifierKeys GetBrightnessDownModifiers()
    {
        return ParseModifiers(BrightnessDownModifiers);
    }

    public Key GetBrightnessDownKey()
    {
        if (Enum.TryParse<Key>(BrightnessDownKey, out var key))
            return key;
        return Key.Down;
    }

    public ModifierKeys GetUnifiedModifiers() => ParseModifiers(UnifiedHotkeyModifiers);

    public Key GetUnifiedKey()
        => Enum.TryParse<Key>(UnifiedHotkeyKey, out var key) ? key : Key.None;

    public ModifierKeys GetUnifiedBrightnessUpModifiers() => ParseModifiers(UnifiedBrightnessUpModifiers);

    public Key GetUnifiedBrightnessUpKey()
        => Enum.TryParse<Key>(UnifiedBrightnessUpKey, out var key) ? key : Key.Up;

    public ModifierKeys GetUnifiedBrightnessDownModifiers() => ParseModifiers(UnifiedBrightnessDownModifiers);

    public Key GetUnifiedBrightnessDownKey()
        => Enum.TryParse<Key>(UnifiedBrightnessDownKey, out var key) ? key : Key.Down;
    
    private ModifierKeys ParseModifiers(string modifiersStr)
    {
        var modifiers = ModifierKeys.None;
        
        if (string.IsNullOrEmpty(modifiersStr))
            return modifiers;
            
        if (modifiersStr.Contains("Control"))
            modifiers |= ModifierKeys.Control;
        if (modifiersStr.Contains("Alt"))
            modifiers |= ModifierKeys.Alt;
        if (modifiersStr.Contains("Shift"))
            modifiers |= ModifierKeys.Shift;
        if (modifiersStr.Contains("Windows"))
            modifiers |= ModifierKeys.Windows;
            
        return modifiers;
    }
    
    public List<int> GetCycleRefreshRates(string? deviceName = null)
    {
        string ratesStr = CycleRefreshRates;
        if (deviceName != null && MonitorCycleRates.TryGetValue(deviceName, out var dictVal))
        {
            ratesStr = dictVal;
        }

        if (string.IsNullOrEmpty(ratesStr))
            return new List<int>();
            
        return ratesStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var rate) ? rate : 0)
            .Where(r => r > 0)
            .ToList();
    }
    
    public void SetCycleRefreshRates(IEnumerable<int> rates, string? deviceName = null)
    {
        var ratesStr = string.Join(",", rates);
        if (deviceName != null)
        {
            MonitorCycleRates[deviceName] = ratesStr;
        }
        else
        {
            CycleRefreshRates = ratesStr;
        }
    }
}
