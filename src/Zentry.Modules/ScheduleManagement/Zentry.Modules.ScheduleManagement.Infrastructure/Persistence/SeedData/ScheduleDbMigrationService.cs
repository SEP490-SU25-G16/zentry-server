using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.SeedData;

public class ScheduleDbMigrationService(
    IServiceProvider serviceProvider,
    ILogger<ScheduleDbMigrationService> logger,
    IHostEnvironment env)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "Schedule Management database migration and seeding initiated (Development environment).");

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, // Retry 2 times
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential back-off
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception,
                            "Schedule Management DB migration/seed attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                            retryCount, timeSpan.TotalSeconds);
                    });

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var seeder =
                        scope.ServiceProvider.GetRequiredService<ScheduleDbSeeder>();

                    await seeder.SeedAllAsync(false, cancellationToken);
                });
                logger.LogInformation("Schedule Management database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Fatal error during Schedule Management database migration/seeding. Application might not function correctly.");
            }
        }
        else
        {
            logger.LogInformation(
                "Schedule Management database migration and seeding skipped (Non-development environment).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
