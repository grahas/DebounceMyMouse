using System.Collections.ObjectModel;

namespace DebounceMyMouse.Core;
public class DebounceChannel
{
    public Debouncer Debouncer { get; set; }
    public StatsService Stats { get; set; }
    public ObservableCollection<string> Logs { get; set; }

    public DebounceChannel(int debounceMs)
    {
        Debouncer = new Debouncer(debounceMs);
        Stats = new StatsService();
        Logs = new ObservableCollection<string>();
    }
}
