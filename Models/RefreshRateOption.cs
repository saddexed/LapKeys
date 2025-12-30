namespace LapKeys.Models;

/// <summary>
/// Represents a refresh rate option with selection state.
/// </summary>
public class RefreshRateOption : ViewModels.ViewModelBase
{
    private bool _isSelected;
    private bool _isIncludedInCycle = true;

    public int Rate { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsIncludedInCycle
    {
        get => _isIncludedInCycle;
        set => SetProperty(ref _isIncludedInCycle, value);
    }

    public string DisplayText => $"{Rate} Hz";

    public RefreshRateOption(int rate, bool isSelected = false, bool isIncludedInCycle = true)
    {
        Rate = rate;
        IsSelected = isSelected;
        IsIncludedInCycle = isIncludedInCycle;
    }
}
