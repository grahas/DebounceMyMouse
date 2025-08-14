using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DebounceMyMouse.Core;
using DebounceMyMouse.ViewModels;

namespace DebounceMyMouse.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.AttachedToVisualTree += OnAttachedToVisualTree;
            this.Closing += OnWindowClosing;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (DataContext is DebounceMyMouse.ViewModels.MainWindowViewModel vm)
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
    }
}