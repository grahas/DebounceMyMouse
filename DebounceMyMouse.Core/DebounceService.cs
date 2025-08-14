using System;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace DebounceMyMouse.Core;
public class DebounceService
{
    private readonly ConcurrentDictionary<string, InputChannel> _channels = new();
    private const int defaultDebounceMs = 50;
    public DebounceService(IEnumerable<InputConfig> configs)
    {
        // Initialize channels based on provided configurations if they are enabled
        SetConfigs(configs);
    }

    public void SetConfigs(IEnumerable<InputConfig> configs)
    {
        // Clear existing channels
        _channels.Clear();
        // Reinitialize channels based on new configurations
        foreach (var inputConfig in configs)
        {
            // Skip disabled inputs
            if (inputConfig.IsEnabled == false)
                continue;
            _channels[inputConfig.Name] = new InputChannel(inputConfig.DebounceMs);
        }
    }

    public void UpdateDebounceTime(string channelName, int debounceMs)
    {
        if (_channels.TryGetValue(channelName, out var channel))
        {
            channel.Debouncer.UpdateDebounceTime(debounceMs);
        }
    }

    public bool ShouldBlock(string channelName)
    {
        if (_channels.TryGetValue(channelName, out var channel))
        {
            return !channel.Debouncer.ShouldTrigger();
        }
        return false;
    }

    public StatsService? GetStats(string channelName)
    {
        return _channels.TryGetValue(channelName, out var channel) ? channel.Stats : null;
    }
    public Dictionary<string, StatsService> GetAllStats()
    {
        // Aggregate stats into a dictionary and save
        var allStats = new Dictionary<string, StatsService>();
        foreach (var kvp in _channels)
        {
            var channelName = kvp.Key;
            var stats = kvp.Value.Stats;
            if (stats != null)
            {
                allStats[channelName] = stats;
            }
        }
        return allStats;
    }

    public void SaveStats(string path)
    {
        StatsService.SaveAllStats(GetAllStats(), path);
    }
}