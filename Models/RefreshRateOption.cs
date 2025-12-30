namespace LapKeys.Models;

/// <summary>
/// Represents a refresh rate option with selection state.
/// </summary>
public class RefreshRateOption : ViewModels.ViewModelBase
{
    private bool _isSelected;

    public int Rate { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string DisplayText => $"{Rate} Hz";

    public RefreshRateOption(int rate, bool isSelected = false)
    {
        Rate = rate;
        IsSelected = isSelected;
    }
}
