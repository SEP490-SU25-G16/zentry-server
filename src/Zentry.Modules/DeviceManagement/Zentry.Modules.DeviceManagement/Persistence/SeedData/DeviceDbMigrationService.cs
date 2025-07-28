using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Zentry.Modules.DeviceManagement.Persistence.SeedData;

public class DeviceDbMigrationService(
    IServiceProvider serviceProvider,
    ILogger<DeviceDbMigrationService> logger,
    IHostEnvironment env)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "Device Management database migration and seeding initiated (Development environment).");

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, // Retry 3 times (Device module có thể cần chờ User module seed xong)
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential back-off
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception,
                            "Device Management DB migration/seed attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                            retryCount, timeSpan.TotalSeconds);
                    });

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var seeder = scope.ServiceProvider.GetRequiredService<DeviceDbSeeder>();

                    await seeder.SeedAllAsync(false, cancellationToken);
                });
                logger.LogInformation("Device Management database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Fatal error during Device Management database migration/seeding. Application might not function correctly.");
            }
        }
        else
        {
            logger.LogInformation(
                "Device Management database migration and seeding skipped (Non-development environment).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
