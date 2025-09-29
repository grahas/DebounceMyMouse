using CommunityToolkit.Mvvm.ComponentModel;

namespace DebounceMyMouse.UI.Models;

public partial class Input : ObservableObject
{
    [ObservableProperty] private string name;
    [ObservableProperty] private int debounceMs;
    [ObservableProperty] private bool isEnabled;

    public Input(string name, int debounceMs, bool isEnabled)
    {
        this.name = name;
        this.debounceMs = debounceMs;
        this.isEnabled = isEnabled;
    }
}
