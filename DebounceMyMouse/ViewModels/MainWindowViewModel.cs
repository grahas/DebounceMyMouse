using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DebounceMyMouse.Core;

namespace DebounceMyMouse.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand IncrementDebounceCommand { get; }
        public ICommand DecrementDebounceCommand { get; }
        public ObservableCollection<MouseEventLogEntry> MouseEventLogs { get; } = new();
        public DebounceConfig? DebounceConfig { get; set; }
        private readonly DebounceBackgroundService _backgroundService;
        public ObservableCollection<InputConfig> Inputs { get; }
        private readonly Dictionary<string, DateTime> _lastEventTimestamps = new();
        public ICommand ClearLogCommand { get; }
        public ICommand LearnCommand { get; }
        public ICommand SaveCommand { get; }

        public MainWindowViewModel(DebounceBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
            DebounceConfig = _backgroundService.config;
            Inputs = new ObservableCollection<InputConfig>(DebounceConfig.Inputs);
            IncrementDebounceCommand = new RelayCommand<InputConfig>(input => input.DebounceMs += 1);
            DecrementDebounceCommand = new RelayCommand<InputConfig>(input => { if (input.DebounceMs > 1) input.DebounceMs -= 1; });
            SaveCommand = new RelayCommand(SaveSettings);
            ClearLogCommand = new RelayCommand(ClearLog);
            LearnCommand = new RelayCommand(Learn);
        }

        private void SaveSettings()
        {
            DebounceConfig.Save("user_settings.json");
            _backgroundService.ReloadConfig();
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

        public class MouseEventLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string InputType { get; set; }
            public bool IsBlocked { get; set; }
            public TimeSpan? Delta { get; set; }

            public string LogMessage {
                get
                {
                    string results = $"{Timestamp:HH:mm:ss.fff} - {InputType}";
                    if (IsBlocked && Delta.HasValue)
                        results += $" (Δ {Delta.Value.TotalMilliseconds:0} ms)";
                    return results;
                }
            }

            public string BackgroundColor => IsBlocked ? "#FFFFCCCC" : "White";
        }

        public void AddMouseEventLog(MouseInputType mouseInputType, bool isBlocked)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                DateTime now = DateTime.Now;
                string inputType = mouseInputType.ToString();
                TimeSpan? delta = null;

                // Only log if the input is enabled
                var config = Inputs.FirstOrDefault(i => i.Name == inputType);
                if (config == null || !config.IsEnabled)
                    return;

                if (_lastEventTimestamps.TryGetValue(inputType, out var lastTimestamp))
                    delta = now - lastTimestamp;

                var entry = new MouseEventLogEntry
                {
                    Timestamp = now,
                    InputType = inputType,
                    IsBlocked = isBlocked,
                    Delta = isBlocked ? delta : null // Only set delta for blocked events
                };

                MouseEventLogs.Add(entry);
                _lastEventTimestamps[inputType] = now;
            });
        }
    }
}


