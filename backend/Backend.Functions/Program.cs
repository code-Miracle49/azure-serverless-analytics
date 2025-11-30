using Backend.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Register application services
        services.AddSingleton<EventService>();
        services.AddSingleton<EnrichmentService>();

        // Register StorageService with configuration from environment
        services.AddSingleton(sp =>
        {
            var accountName = Environment.GetEnvironmentVariable("StorageAccountName")
                ?? throw new InvalidOperationException("StorageAccountName not configured");
            var accountKey = Environment.GetEnvironmentVariable("StorageAccountKey")
                ?? throw new InvalidOperationException("StorageAccountKey not configured");

            return new StorageService(accountName, accountKey);
        });

        // Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

host.Run();