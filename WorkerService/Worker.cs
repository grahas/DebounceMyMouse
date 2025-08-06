using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private DebounceService? _debounceService;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DebounceConfig config;
            try
            {
                config = DebounceConfig.Load("debounceConfig.json");
                _logger.LogInformation("Config loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load config.");
                return;
            }

            _debounceService = new DebounceService(config.Inputs ?? new List<InputConfig>());

            // Subscribe to mouse input events
            MouseHook.OnInput += input =>
            {
                _debounceService.HandleInput(input.ToString()); // Use input.ToString() if using string-based channels
                _logger.LogInformation("Mouse input received: {input}", input);
            };

            MouseHook.Start();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Optionally, log stats or perform other background tasks
                    await Task.Delay(1000, stoppingToken);
                }
            }
            finally
            {
                MouseHook.Stop();
            }
        }
    }
}