using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DebounceMyMouse.ViewModels;
using DebounceMyMouse.Views;
using System.IO;
using DebounceMyMouse.Core;


namespace DebounceMyMouse
{
    public partial class App : Application
    {
        private DebounceBackgroundService? _backgroundService;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var config = DebounceConfig.Load("debounceConfig.json");
            _backgroundService = new DebounceBackgroundService(config);
            _backgroundService.Start();

            base.OnFrameworkInitializationCompleted();
        }

        private void OnOpenSettings(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Windows.FirstOrDefault(w => w is MainWindow) is not MainWindow mainWindow)
                {
                    mainWindow = new MainWindow(Config);
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                }
                else
                {
                    mainWindow.Activate();
                }
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