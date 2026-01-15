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
    
    public string CycleRefreshRates { get; set; } = "";
    
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
    
    public Key GetKey()
    {
        if (Enum.TryParse<Key>(HotkeyKey, out var key))
            return key;
        return Key.R;
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
    
    public void SetCycleRefreshRates(IEnumerable<int> rates)
    {
        CycleRefreshRates = string.Join(",", rates);
    }
}
