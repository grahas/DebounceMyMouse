using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace DebounceMyMouse.UI
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            using var single = new SingleInstance("DebounceMyMouse");

            if (!single.IsPrimary)
            {
                single.NotifyFirstInstance(args);
                return 0;
            }

            single.ArgumentsReceived += a =>
            {
                // Bring main window to front and pass args
                Dispatcher.UIThread.Post(() =>
                {
                    var win = (Application.Current?.ApplicationLifetime as
                               Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime)
                              ?.MainWindow;
                    if (win is null) return;
                    if (win.WindowState == WindowState.Minimized) win.WindowState = WindowState.Normal;
                    win.Show();
                    win.Activate();
                    // TODO: handle args if needed
                });
            };

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
