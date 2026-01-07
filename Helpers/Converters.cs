using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LapKeys.Helpers;

/// <summary>
/// Converts boolean to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Converts boolean to custom text. Use ConverterParameter with format "TrueText|FalseText".
/// Example: ConverterParameter="Cancel|Set" shows "Cancel" when true, "Set" when false.
/// Default: "Cancel" when true, "Set" when false.
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Default texts
        string trueText = "Cancel";
        string falseText = "Set";

        // Parse custom texts from parameter if provided (format: "TrueText|FalseText")
        if (parameter is string paramStr && paramStr.Contains('|'))
        {
            var parts = paramStr.Split('|');
            if (parts.Length >= 2)
            {
                trueText = parts[0];
                falseText = parts[1];
            }
        }

        if (value is bool boolValue)
        {
            return boolValue ? trueText : falseText;
        }

        return falseText;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Context-aware converter for hotkey capture buttons.
/// Uses MultiBinding with: IsCapturingHotkey, CapturingHotkeyType
/// ConverterParameter: the button's hotkey type (e.g., "CycleRefreshRate")
/// Shows "Cancel" only when THIS button is the one being captured.
/// </summary>
public class HotkeyButtonTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = IsCapturingHotkey (bool)
        // values[1] = CapturingHotkeyType (string)
        // parameter = this button's hotkey type (string)
        
        if (values.Length < 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            return "Set";
        
        bool isCapturing = values[0] is bool b && b;
        string capturingType = values[1] as string ?? string.Empty;
        string buttonType = parameter as string ?? string.Empty;
        
        // Only show "Cancel" if capturing is active AND this button started it
        if (isCapturing && capturingType == buttonType)
            return "Cancel";
        
        return "Set";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
