using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public class ConfigurationDbMigrationService(
    IServiceProvider serviceProvider,
    ILogger<ConfigurationDbMigrationService> logger,
    IHostEnvironment env)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "Configuration Management database migration and seeding initiated (Development environment).");

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, // Thử lại 2 lần
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Thời gian đợi lũy thừa
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception,
                            "Configuration DB migration/seed attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                            retryCount, timeSpan.TotalSeconds);
                    });

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var seeder = scope.ServiceProvider.GetRequiredService<ConfigurationDbSeeder>();

                    await seeder.SeedAllAsync(); // Không recreate database
                });
                logger.LogInformation(
                    "Configuration Management database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Fatal error during Configuration Management database migration/seeding. Application might not function correctly.");
            }
        }
        else
        {
            logger.LogInformation(
                "Configuration Management database migration and seeding skipped (Non-development environment).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}