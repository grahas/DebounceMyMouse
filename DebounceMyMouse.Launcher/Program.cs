using System;
using System.Diagnostics;
using System.IO;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        // In MSIX this resolves to ...\WindowsApps\...\DebounceMyMouse.Launcher\
        var baseDir = AppContext.BaseDirectory;

        // Normalize and compute a syntactic parent; stays at root if already at root.
        var parentDir = Path.GetFullPath(Path.Combine(baseDir, ".."));

        bool atRoot = string.Equals(
            Path.TrimEndingDirectorySeparator(parentDir),
            Path.GetPathRoot(parentDir),
            StringComparison.OrdinalIgnoreCase
        );

        // Use parentDir, but handle `atRoot` if that matters for your logic.
        var targetExe = Path.Combine(parentDir, "DebounceMyMouse.UI", "DebounceMyMouse.UI.exe");

        // Fallback for non-packaged/dev runs (same folder)
        if (!File.Exists(targetExe))
        {
            var sameDir = Path.Combine(baseDir, "DebounceMyMouse.UI.exe");
            if (File.Exists(sameDir))
                targetExe = sameDir;
        }

        var psi = new ProcessStartInfo(targetExe, "--minimized")
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(targetExe)!
        };

        Process.Start(psi);
    }
}