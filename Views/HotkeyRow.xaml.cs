using System.Windows;
using System.Windows.Controls;

namespace LapKeys.Views;

public partial class HotkeyRow : System.Windows.Controls.UserControl
{
    public HotkeyRow()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(HotkeyRow), 
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(HotkeyRow), 
            new PropertyMetadata(string.Empty));

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public static readonly DependencyProperty HotkeyTypeProperty =
        DependencyProperty.Register(nameof(HotkeyType), typeof(string), typeof(HotkeyRow), 
            new PropertyMetadata(string.Empty));

    public string HotkeyType
    {
        get => (string)GetValue(HotkeyTypeProperty);
        set => SetValue(HotkeyTypeProperty, value);
    }
}
