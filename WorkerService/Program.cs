using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices; // Add this using directive
using WorkerService;

Host.CreateDefaultBuilder(args)
    .UseWindowsService() // Important: runs as a Windows Service
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();