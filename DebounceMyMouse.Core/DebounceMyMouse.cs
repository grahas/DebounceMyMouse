using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebounceMyMouse.Core
{
    public class DebounceMyMouse
    {
        public AppSettings Settings { get; private set; }
        public static event Action<string, bool>? OnMouseInputResults;

        private List<Channel> Debouncers = new List<Channel>();
        private MouseHook MouseHook = new MouseHook();

        public DebounceMyMouse() 
        {
            // Read the settings from disk
            Settings = AppSettings.Load();

            // If first run, set some defaults
            if (Settings.IsFirstRun)
            {
                Settings.IsFirstRun = false;
                Settings.LaunchOnStartup = true;
                Settings.Channels.Add(new ChannelSetting("Left", TimeSpan.FromMilliseconds(200)));
                Settings.Channels.Add(new ChannelSetting("Right", TimeSpan.FromMilliseconds(200)));
                Settings.Channels.Add(new ChannelSetting("Middle", TimeSpan.FromMilliseconds(200)));
                Settings.Save();
            }

            // Initialize debouncers based on settings
            Debouncers = Settings.Channels
                .Select(c => new Channel(c.Name, c.Timeout))
                .ToList();

            // Hook the mouse events
            MouseHook.ShouldBlock += ShouldBlock;
            MouseHook.Start();
        }

        ~DebounceMyMouse()
        {
            MouseHook.Stop();
        }

        private static Task NotifyMouseInputResultsAsync(string input, bool results)
        {
            var handlers = OnMouseInputResults;
            if (handlers is null) return Task.CompletedTask;

            var invocations = handlers.GetInvocationList();
            var tasks = new Task[invocations.Length];

            for (int i = 0; i < invocations.Length; i++)
            {
                var d = (Action<string, bool>)invocations[i];
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        d(input, results);
                    }
                    catch
                    {
                        // Swallow or log individual handler exceptions as needed
                    }
                });
            }

            return Task.WhenAll(tasks);
        }

        private bool ShouldBlock(string input)
        {
            bool results;
            var channel = Debouncers.FirstOrDefault(c => c.Name == input);
            if (channel != null)
            {
                results = channel.ShouldBlock();
                _ = NotifyMouseInputResultsAsync(input, results); // async, fire-and-forget

            }
            else
            {
                results = false;
            }

            return results;
        }

        public void ApplySettings()
        {
            Settings = AppSettings.Load();
            Debouncers.Clear();
            Debouncers = Settings.Channels
                .Select(c => new Channel(c.Name, c.Timeout))
                .ToList();
        }
    }
}
