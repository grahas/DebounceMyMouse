using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DebounceMyMouse.Core;
using DebounceMyMouse.UI.ViewModels;
using System.Reactive.Linq;

namespace DebounceMyMouse.UI.Views
{
    public partial class MainWindow : Window
    {
        private IDisposable? _visSub;

        public MainWindow()
        {
            InitializeComponent();

            _visSub = this.GetObservable(Visual.IsVisibleProperty)
            .Subscribe(visible =>
            {
                if (visible) OnShown();
                else OnHidden();
            });


            this.AttachedToVisualTree += OnAttachedToVisualTree;
            this.Closing += OnWindowClosing;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (DataContext is DebounceMyMouse.UI.ViewModels.MainWindowViewModel vm)
            {
                vm.MouseEventLogs.CollectionChanged += MouseEventLogs_CollectionChanged;
            }
        }

        private void MouseEventLogs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (EventLogListBox.ItemCount > 0)
            {
                var lastIndex = EventLogListBox.ItemCount - 1;
                EventLogListBox.SelectedIndex = lastIndex;
                EventLogListBox.ScrollIntoView(EventLogListBox.SelectedItem);
            }
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cancel the close and hide the window instead
            e.Cancel = true;
            this.Hide();
        }

        private void OnShown() 
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    if (vm.debounceMyMouse != null)
                    {
                        vm.MouseEventLogs.Clear();
                        DebounceMyMouse.Core.DebounceMyMouse.OnMouseInputResults -= vm.AddMouseEventLog;
                        DebounceMyMouse.Core.DebounceMyMouse.OnMouseInputResults += vm.AddMouseEventLog;
                    }
                }
            });
        }

        private void OnHidden() 
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    if (vm.debounceMyMouse != null)
                    {
                        DebounceMyMouse.Core.DebounceMyMouse.OnMouseInputResults -= vm.AddMouseEventLog;
                    }
                }
            });
        }
    }
}