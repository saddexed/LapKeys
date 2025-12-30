using System.Windows.Input;

namespace LapKeys.Models;

/// <summary>
/// Represents a hotkey configuration with modifiers and key.
/// </summary>
public class HotkeyBinding
{
    public string Name { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public ModifierKeys Modifiers { get; set; }
    public Key Key { get; set; }
    public int Id { get; set; }

    public HotkeyBinding() { }

    public HotkeyBinding(string name, string action, ModifierKeys modifiers, Key key, int id)
    {
        Name = name;
        Action = action;
        Modifiers = modifiers;
        Key = key;
        Id = id;
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (Modifiers.HasFlag(ModifierKeys.Windows))
            parts.Add("Win");
        if (Modifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");
        if (Modifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");

        parts.Add(Key.ToString());

        return string.Join(" + ", parts);
    }

    public static HotkeyBinding Parse(string hotkeyString, string name, string action, int id)
    {
        var binding = new HotkeyBinding
        {
            Name = name,
            Action = action,
            Id = id,
            Modifiers = ModifierKeys.None,
            Key = Key.None
        };

        var parts = hotkeyString.Split('+', StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            switch (part.ToUpperInvariant())
            {
                case "WIN":
                case "WINDOWS":
                    binding.Modifiers |= ModifierKeys.Windows;
                    break;
                case "CTRL":
                case "CONTROL":
                    binding.Modifiers |= ModifierKeys.Control;
                    break;
                case "ALT":
                    binding.Modifiers |= ModifierKeys.Alt;
                    break;
                case "SHIFT":
                    binding.Modifiers |= ModifierKeys.Shift;
                    break;
                default:
                    if (Enum.TryParse<Key>(part, true, out var key))
                        binding.Key = key;
                    break;
            }
        }

        return binding;
    }
}
