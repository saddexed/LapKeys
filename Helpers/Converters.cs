using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LapKeys.Helpers;

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

public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string trueText = "Cancel";
        string falseText = "Set";

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

public class HotkeyButtonTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        
        if (values.Length < 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            return "Set";
        
        bool isCapturing = values[0] is bool b && b;
        string capturingType = values[1] as string ?? string.Empty;
        
        string buttonType = values.Length >= 3 && values[2] != DependencyProperty.UnsetValue
            ? values[2] as string ?? string.Empty
            : parameter as string ?? string.Empty;
        
        if (isCapturing && capturingType == buttonType)
            return "Cancel";
        
        return "Set";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
