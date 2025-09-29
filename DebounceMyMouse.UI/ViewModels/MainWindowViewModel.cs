using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebounceMyMouse.Core;
using DebounceMyMouse.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;

namespace DebounceMyMouse.UI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MouseEventLogEntry> MouseEventLogs { get; } = new();
        private readonly Dictionary<string, DateTime> _lastEventTimestamps = new();
        public ICommand ClearLogCommand { get; }
        public ICommand LearnCommand { get; }
        public ICommand SaveCommand { get; }

        public DebounceMyMouse.Core.DebounceMyMouse debounceMyMouse = new();

        [ObservableProperty]
        private bool launchOnStartup;
        private bool _suppressLaunchChange;

        public ObservableCollection<Input> Inputs { get; set; } = new()
        {
            new Input("Left", 200, true),
            new Input("Right", 200, true),
            new Input("Middle", 200, true)
        };

        public MainWindowViewModel()
        {
            SaveCommand = new RelayCommand(SaveSettings);
            ClearLogCommand = new RelayCommand(ClearLog);
            LearnCommand = new RelayCommand(Learn);

            // Load settings, if setting isn't found, use default value and disable it
            foreach (var input in Inputs)
            {
                var setting = debounceMyMouse.Settings.Channels
                    .FirstOrDefault(c => c.Name.Equals(input.Name, StringComparison.OrdinalIgnoreCase));
                if (setting != null)
                {
                    input.DebounceMs = (int)setting.Timeout.TotalMilliseconds;
                    input.IsEnabled = true;
                }
                else
                {
                    input.DebounceMs = 200; // default value
                    input.IsEnabled = false;
                }
            }

            // Initialize LaunchOnStartup from settings
            LaunchOnStartup = debounceMyMouse.Settings.LaunchOnStartup;
        }

        private void SaveSettings()
        {
            // Update settings from Inputs collection, only add enabled inputs
            debounceMyMouse.Settings.Channels = Inputs
                .Where(i => i.IsEnabled)
                .Select(i => new ChannelSetting(i.Name, TimeSpan.FromMilliseconds(i.DebounceMs)))
                .ToList();

            // Persist LaunchOnStartup to settings
            debounceMyMouse.Settings.LaunchOnStartup = LaunchOnStartup;

            debounceMyMouse.Settings.Save();
            debounceMyMouse.ApplySettings();
        }

        private void ClearLog()
        {
            MouseEventLogs.Clear();
        }

        public void Learn()
        {
            // Group log entries by InputType
            var grouped = MouseEventLogs
                .Where(e => e.IsBlocked && e.Delta.HasValue)
                .GroupBy(e => e.InputType);

            foreach (var group in grouped)
            {
                var deltas = group.Select(e => e.Delta.Value.TotalMilliseconds).ToList();
                if (deltas.Count == 0)
                    continue;

                double mean = deltas.Average();
                double sumSq = deltas.Sum(d => Math.Pow(d - mean, 2));
                double stddev = Math.Sqrt(sumSq / deltas.Count);
                double threshold = mean + 3 * stddev;

                // Update the corresponding InputConfig
                var config = Inputs.FirstOrDefault(i => i.Name == group.Key);
                if (config != null)
                {
                    config.DebounceMs = (int)Math.Round(threshold);
                }
            }
        }

        public void AddMouseEventLog(string input, bool isBlocked)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                DateTime now = DateTime.Now;
                TimeSpan? delta = null;

                if (_lastEventTimestamps.TryGetValue(input, out var lastTimestamp))
                    delta = now - lastTimestamp;

                var entry = new MouseEventLogEntry
                {
                    Timestamp = now,
                    InputType = input,
                    IsBlocked = isBlocked,
                    Delta = isBlocked ? delta : null // Only set delta for blocked events
                };

                MouseEventLogs.Add(entry);
                _lastEventTimestamps[input] = now;
            });
        }

        // Keep settings in sync when LaunchOnStartup changes
        partial void OnLaunchOnStartupChanged(bool value)
        {
            if (_suppressLaunchChange) return;          // prevent loops
            _ = ApplyLaunchOnStartupAsync(value);       // fire-and-forget
        }

        private async Task ApplyLaunchOnStartupAsync(bool requested)
        {
            // MSIX only; bail out cleanly when unpackaged
            if (!StartupToggle.IsSupported())
            {
                await StartupToggle.DisableAsync();
                await ReflectAsync(false);
                return;
            }

            bool enabled;
            if (requested)
            {
                // May prompt; returns false if DisabledByUser/Policy
                enabled = await StartupToggle.EnableAsync();
                if (!enabled && await StartupToggle.GetStateAsync() == StartupTaskState.DisabledByUser)
                {
                    // optionally notify user to re-enable in Settings > Apps > Startup
                }
            }
            else
            {
                await StartupToggle.DisableAsync();
                enabled = false;
            }

            await ReflectAsync(enabled);
        }

        private Task ReflectAsync(bool actual)
        {
            // update settings and UI without re-entering the callback
            debounceMyMouse.Settings.LaunchOnStartup = actual;

            if (LaunchOnStartup != actual)
            {
                _suppressLaunchChange = true;
                LaunchOnStartup = actual;
                _suppressLaunchChange = false;
            }
            return Task.CompletedTask;
        }
    }
}


