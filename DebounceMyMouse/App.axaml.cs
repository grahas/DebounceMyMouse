using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices; // Add this for OS check
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DebounceMyMouse.Core;
using DebounceMyMouse.ViewModels;
using DebounceMyMouse.Views;

namespace DebounceMyMouse
{
    public partial class App : Application
    {
        private DebounceBackgroundService? _backgroundService;
        private MainWindowViewModel mainWindowViewModel;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDeveloperTools();

        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            _backgroundService = new DebounceBackgroundService();
            mainWindowViewModel = new MainWindowViewModel(_backgroundService);
            _backgroundService.OnMouseInputResults = mainWindowViewModel.AddMouseEventLog;
            _backgroundService.Start();

            base.OnFrameworkInitializationCompleted();
        }

        private void OnOpenSettings(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Try to find the main window
                var mainWindow = desktop.Windows.FirstOrDefault(w => w is MainWindow) as MainWindow;

                if (mainWindow == null)
                {
                    // If not found, create a new one
                    mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };
                    desktop.MainWindow = mainWindow;
                }

                // Show and activate the window
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            }
        }

        private void OnExitApp(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Stop the background service
                _backgroundService?.Stop();
                desktop.Shutdown();
            }
        }


        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}