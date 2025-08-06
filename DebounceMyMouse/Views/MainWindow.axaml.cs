using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DebounceMyMouse.Core;
using DebounceMyMouse.ViewModels;

namespace DebounceMyMouse.Views
{
    public partial class MainWindow : Window
    {
        private Debouncer _debouncer = new Debouncer(50);
        private StatsService _stats = new StatsService();
        private Dictionary<MouseInputType, DebounceChannel> _channels = new();
        
        private MouseInputType _currentSelectedInput = MouseInputType.Left;
        public ObservableCollection<string> LogEntries { get; } = new();

        public MainWindow(DebounceConfig config)
        {
            InitializeComponent();
            InputConfigs = new ObservableCollection<InputConfig>(config.Inputs ?? new List<InputConfig>());
            DataContext = new MainWindowViewModel(config)
            LogList.ItemsSource = LogEntries;

            foreach (MouseInputType input in Enum.GetValues(typeof(MouseInputType)))
            {
                _channels[input] = new DebounceChannel(50); // Default debounce time
            }

            MouseHook.OnInput += (input) =>
            {
                var channel = _channels[input];

                if (channel.Debouncer.ShouldTrigger())
                {
                    channel.Stats.LogBounce();
                    Dispatcher.UIThread.Post(() =>
                    {
                        var line = $"{input} at {DateTime.Now:T}";
                        channel.Logs.Insert(0, line);

                        if (channel.Logs.Count > 100)
                            channel.Logs.RemoveAt(channel.Logs.Count - 1);

                        if (input == _currentSelectedInput) // Displaying selected input
                        {
                            LogList.ItemsSource = channel.Logs;
                            StatsDisplay.Text = $"[{input}] Count: {channel.Stats.BounceCount}, Avg: {channel.Stats.AverageInterval:F0} ms";
                        }
                    });
                }
            };
        }

        private void Apply_Click(object? sender, RoutedEventArgs e)
        {
            _debouncer.UpdateDebounceTime((int)DebounceTimeInput.Value);
        }
        private void InputSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //if (InputSelector.SelectedItem is MouseInputType input)
            //{
            //    _currentSelectedInput = input;
            //    var channel = _channels[input];
            //    LogList.ItemsSource = channel.Logs;
            //    StatsDisplay.Text = $"[{input}] Count: {channel.Stats.BounceCount}, Avg: {channel.Stats.AverageInterval:F0} ms";
            //}
        }
        private void Save_Click(object? sender, RoutedEventArgs e)
        {
            var config = new DebounceConfig { Inputs = InputConfigs.ToList() };
            config.Save("debounceConfig.json");
        }

        public static class MouseInputTypeEnumValues
        {
            public static IEnumerable<string> Values => Enum.GetNames(typeof(MouseInputType));
        }
    }

}



