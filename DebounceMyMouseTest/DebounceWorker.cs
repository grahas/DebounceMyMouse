using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DebounceWorker : BackgroundService
{
    private readonly ILogger<DebounceWorker> _logger;
    private readonly DebounceService _debounceService;

    public DebounceWorker(ILogger<DebounceWorker> logger)
    {
        _logger = logger;
        _debounceService = new DebounceService();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Debounce Service running.");

        // Example: poll or hook mouse input here
        while (!stoppingToken.IsCancellationRequested)
        {
            // Your debouncing logic here
            // e.g., _debounceService.CheckInputs();

            await Task.Delay(100, stoppingToken);
        }
    }
}