using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public class StatsService
{
    public List<long> Intervals = new();
    private Stopwatch _stopwatch = Stopwatch.StartNew();
    public int BounceCount { get; set; }

    public void LogBounce()
    {
        Intervals.Add(_stopwatch.ElapsedMilliseconds);
        _stopwatch.Restart();
        BounceCount++;
    }

    public double AverageInterval => Intervals.Count > 0 ? Intervals.Average() : 0;

    // Static: Save all stats to a single file
    public static void SaveAllStats(Dictionary<string, StatsService> allStats, string path)
    {
        var dict = new Dictionary<string, StatsData>();
        foreach (var kvp in allStats)
        {
            dict[kvp.Key] = new StatsData
            {
                BounceCount = kvp.Value.BounceCount,
                Intervals = kvp.Value.Intervals
            };
        }
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    // Static: Load all stats from a single file
    public static void LoadAllStats(Dictionary<string, StatsService> allStats, string path)
    {
        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<Dictionary<string, StatsData>>(json);
        if (dict == null)
            return;

        foreach (var kvp in dict)
        {
            if (!allStats.TryGetValue(kvp.Key, out var statsService))
            {
                statsService = new StatsService();
                allStats[kvp.Key] = statsService;
            }
            statsService.BounceCount = kvp.Value.BounceCount;
            statsService.Intervals = kvp.Value.Intervals ?? new List<long>();
        }
    }

    private class StatsData
    {
        public int BounceCount { get; set; }
        public List<long> Intervals { get; set; }
    }
}