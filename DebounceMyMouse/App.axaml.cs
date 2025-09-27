using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DebounceMyMouse.Core;
using DebounceMyMouse.UI.ViewModels;
using DebounceMyMouse.UI.Views;
using DebounceMyMouse.Core;

namespace DebounceMyMouse.UI
{
    public partial class App : Application
    {
        private MainWindowViewModel mainWindowViewModel;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDeveloperTools();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            mainWindowViewModel = new MainWindowViewModel();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Keep app running when no windows are open (tray-only scenario).
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var args = desktop.Args ?? Array.Empty<string>();
                bool HasArg(string v) => args.Any(a => string.Equals(a, v, StringComparison.OrdinalIgnoreCase));

                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
                desktop.MainWindow = mainWindow;

                // Check if the window should start minimized
                if (HasArg("--minimized") == false)
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                }
            }

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