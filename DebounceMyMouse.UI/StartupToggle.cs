using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

public static class StartupToggle
{
    private const string StartupId = "DebounceMyMouse_Startup";

    public static bool IsSupported()
    {
        // MSIX only. Package.Current throws if unpackaged.
        try { _ = Package.Current; return true; }
        catch { return false; }
    }

    public static async Task<StartupTaskState> GetStateAsync()
    {
        if (!IsSupported()) return StartupTaskState.DisabledByPolicy;
        var task = await StartupTask.GetAsync(StartupId);
        return task.State;
    }

    public static async Task<bool> EnableAsync()
    {
        if (!IsSupported()) return false;

        var task = await StartupTask.GetAsync(StartupId);
        // First time may prompt; if user disabled in Task Manager, cannot auto-enable.
        var state = task.State switch
        {
            StartupTaskState.Disabled => await task.RequestEnableAsync(),
            StartupTaskState.DisabledByPolicy => StartupTaskState.DisabledByPolicy,
            StartupTaskState.DisabledByUser => StartupTaskState.DisabledByUser,
            _ => task.State
        };
        return state == StartupTaskState.Enabled;
    }

    public static async Task DisableAsync()
    {
        if (!IsSupported()) return;
        var task = await StartupTask.GetAsync(StartupId);
        task.Disable();
    }
}
