namespace LapKeys.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _title = "LapKeys - Laptop Control";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public MainViewModel()
    {
        // Initialize properties here
    }
}
