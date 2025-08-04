using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class StatsService
{
    public List<long> Intervals = new();
    private Stopwatch _stopwatch = Stopwatch.StartNew();
    public int BounceCount { get; private set; }

    public void LogBounce()
    {
        Intervals.Add(_stopwatch.ElapsedMilliseconds);
        _stopwatch.Restart();
        BounceCount++;
    }

    public double AverageInterval => Intervals.Count > 0 ? Intervals.Average() : 0;
}