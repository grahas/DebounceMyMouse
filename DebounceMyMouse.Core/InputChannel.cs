using System.Collections.ObjectModel;

namespace DebounceMyMouse.Core;
public class InputChannel
{
    public Debouncer Debouncer { get; set; }
    public StatsService Stats { get; set; }
    public ObservableCollection<string> Logs { get; set; }

    public InputChannel(int debounceMs)
    {
        Debouncer = new Debouncer(debounceMs);
        Stats = new StatsService();
        Logs = new ObservableCollection<string>();
    }
}
