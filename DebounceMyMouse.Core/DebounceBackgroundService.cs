namespace DebounceMyMouse.Core;
public class DebounceBackgroundService
{
    private CancellationTokenSource? _cts;
    private Task? _backgroundTask;
    private DebounceService _debounceService;
    public Action<MouseInputType, bool>? OnMouseInputResults;
    private readonly string _configPath;
    public DebounceConfig config { get; set; }
    public DebounceBackgroundService(string configPath = null)
    {
        // Use a safe, user-writable location for config
        if (string.IsNullOrEmpty(configPath))
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "DebounceMyMouse");
            Directory.CreateDirectory(appFolder);
            configPath = Path.Combine(appFolder, "user_settings.json");
        }
        _configPath = configPath;
        config = DebounceConfig.Load(_configPath);
        _debounceService = new DebounceService(config.Inputs ?? new List<InputConfig>());
    }

    public void Start()
    {
        if (_backgroundTask != null && !_backgroundTask.IsCompleted)
            return; // Already running

        _cts = new CancellationTokenSource();
        
        // Assign callback functions
        MouseHook.ShouldBlock += ShouldBlock;
        if (OnMouseInputResults != null)
        {
            MouseHook.OnMouseInputResults += OnMouseInputResults;
        }
        else
        {
            MouseHook.OnMouseInputResults += (input, blocked) => _debounceService.GetStats(input.ToString())?.LogBounce();
        }
        MouseHook.Start();

        _backgroundTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                // Optionally log stats or perform other background tasks
                await Task.Delay(1000, _cts.Token);
            }
        }, _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        MouseHook.ShouldBlock -= ShouldBlock;
        MouseHook.OnMouseInputResults -= OnMouseInputResults;
        MouseHook.Stop();

        // Save the stats to a file
        _debounceService.SaveStats(_configPath);
    }

    public void ReloadConfig()
    {
        // Reload the configuration
        config = DebounceConfig.Load(_configPath);
        _debounceService = new DebounceService(config.Inputs ?? new List<InputConfig>());
    }

    private bool ShouldBlock(MouseInputType input)
    {
        bool results = _debounceService.ShouldBlock(input.ToString());

        return results;
    }
}