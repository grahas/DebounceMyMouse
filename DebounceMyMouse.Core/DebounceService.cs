using System;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace DebounceMyMouse.Core;
public class DebounceService
{
    private readonly ConcurrentDictionary<string, DebounceChannel> _channels = new();
    private const int defaultDebounceMs = 50;
    public DebounceService(IEnumerable<InputConfig> configs)
    {
        // Initialize channels based on provided configurations
        foreach (var inputConfig in configs)
        {
            _channels[inputConfig.Name] = new DebounceChannel(inputConfig.DebounceMs);
        }
    }

    public void UpdateDebounceTime(string channelName, int debounceMs)
    {
        if (_channels.TryGetValue(channelName, out var channel))
        {
            channel.Debouncer.UpdateDebounceTime(debounceMs);
        }
    }

    public void HandleInput(string channelName)
    {
        if (_channels.TryGetValue(channelName, out var channel))
        {
            if (channel.Debouncer.ShouldTrigger())
            {
                channel.Stats.LogBounce();
                // Add logging or event notification here if needed
            }
        }
        else
        {
            // Optionally: create channel on the fly
             //_channels[channelName] = new DebounceChannel(defaultDebounceMs);
        }
    }

    public StatsService? GetStats(string channelName)
    {
        return _channels.TryGetValue(channelName, out var channel) ? channel.Stats : null;
    }
}