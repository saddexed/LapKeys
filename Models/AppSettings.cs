using System.Windows.Input;

namespace LapKeys.Models;

/// <summary>
/// Represents the application settings that are persisted.
/// </summary>
public class AppSettings
{
    public bool IsDarkMode { get; set; } = false;
    public bool MinimizeToTrayOnClose { get; set; } = true;
    
    // Hotkey settings
    public string HotkeyModifiers { get; set; } = "Control, Shift";
    public string HotkeyKey { get; set; } = "R";
    
    // Brightness hotkey settings
    public string BrightnessUpModifiers { get; set; } = "Control, Shift";
    public string BrightnessUpKey { get; set; } = "Up";
    public string BrightnessDownModifiers { get; set; } = "Control, Shift";
    public string BrightnessDownKey { get; set; } = "Down";
    
    // Hotkey enabled states
    public bool IsRefreshRateHotkeyEnabled { get; set; } = true;
    public bool IsBrightnessHotkeysEnabled { get; set; } = true;
    
    // Refresh rates included in cycle (stored as comma-separated values)
    public string CycleRefreshRates { get; set; } = "";
    
    /// <summary>
    /// Parses the stored modifiers string to ModifierKeys.
    /// </summary>
    public ModifierKeys GetModifierKeys()
    {
        var modifiers = ModifierKeys.None;
        
        if (string.IsNullOrEmpty(HotkeyModifiers))
            return modifiers;
            
        if (HotkeyModifiers.Contains("Control"))
            modifiers |= ModifierKeys.Control;
        if (HotkeyModifiers.Contains("Alt"))
            modifiers |= ModifierKeys.Alt;
        if (HotkeyModifiers.Contains("Shift"))
            modifiers |= ModifierKeys.Shift;
        if (HotkeyModifiers.Contains("Windows"))
            modifiers |= ModifierKeys.Windows;
            
        return modifiers;
    }
    
    /// <summary>
    /// Parses the stored key string to Key enum.
    /// </summary>
    public Key GetKey()
    {
        if (Enum.TryParse<Key>(HotkeyKey, out var key))
            return key;
        return Key.R;
    }
    
    /// <summary>
    /// Gets brightness up hotkey modifiers.
    /// </summary>
    public ModifierKeys GetBrightnessUpModifiers()
    {
        return ParseModifiers(BrightnessUpModifiers);
    }
    
    /// <summary>
    /// Gets brightness up hotkey key.
    /// </summary>
    public Key GetBrightnessUpKey()
    {
        if (Enum.TryParse<Key>(BrightnessUpKey, out var key))
            return key;
        return Key.Up;
    }
    
    /// <summary>
    /// Gets brightness down hotkey modifiers.
    /// </summary>
    public ModifierKeys GetBrightnessDownModifiers()
    {
        return ParseModifiers(BrightnessDownModifiers);
    }
    
    /// <summary>
    /// Gets brightness down hotkey key.
    /// </summary>
    public Key GetBrightnessDownKey()
    {
        if (Enum.TryParse<Key>(BrightnessDownKey, out var key))
            return key;
        return Key.Down;
    }
    
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
    
    /// <summary>
    /// Gets the list of refresh rates included in the cycle.
    /// </summary>
    public List<int> GetCycleRefreshRates()
    {
        if (string.IsNullOrEmpty(CycleRefreshRates))
            return new List<int>();
            
        return CycleRefreshRates
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var rate) ? rate : 0)
            .Where(r => r > 0)
            .ToList();
    }
    
    /// <summary>
    /// Sets the cycle refresh rates from a list.
    /// </summary>
    public void SetCycleRefreshRates(IEnumerable<int> rates)
    {
        CycleRefreshRates = string.Join(",", rates);
    }
}
