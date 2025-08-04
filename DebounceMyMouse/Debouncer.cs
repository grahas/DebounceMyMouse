using System;
using System.Diagnostics;

public class Debouncer
{
    private Stopwatch _timer = Stopwatch.StartNew();
    private int _debounceMs;

    public Debouncer(int debounceMs)
    {
        _debounceMs = debounceMs;
    }

    public void UpdateDebounceTime(int newMs)
    {
        _debounceMs = newMs;
    }

    public bool ShouldTrigger()
    {
        if (_timer.ElapsedMilliseconds >= _debounceMs)
        {
            _timer.Restart();
            return true;
        }
        return false;
    }
}
