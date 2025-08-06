using System.Threading;
using System.Threading.Tasks;
namespace DebounceMyMouse.Core;
public class DebounceBackgroundService
{
    private CancellationTokenSource? _cts;
    private Task? _backgroundTask;
    private readonly DebounceService _debounceService;

    public DebounceBackgroundService(DebounceConfig config)
    {
        _debounceService = new DebounceService(config.Inputs ?? new List<InputConfig>());
    }

    public void Start()
    {
        if (_backgroundTask != null && !_backgroundTask.IsCompleted)
            return; // Already running

        _cts = new CancellationTokenSource();
        MouseHook.OnInput += OnMouseInput;
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
        MouseHook.OnInput -= OnMouseInput;
        MouseHook.Stop();
    }

    private void OnMouseInput(MouseInputType input)
    {
        _debounceService.HandleInput(input.ToString());
        // Optionally: raise events, log, etc.
    }
}