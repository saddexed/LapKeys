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
