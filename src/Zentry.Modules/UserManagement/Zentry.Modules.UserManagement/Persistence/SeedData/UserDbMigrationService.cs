using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Zentry.Modules.UserManagement.Persistence.SeedData;

public class UserDbMigrationService(
    IServiceProvider serviceProvider,
    ILogger<UserDbMigrationService> logger,
    IHostEnvironment env)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "User Management database migration and seeding initiated (Development environment).");

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception,
                            "User Management DB migration/seed attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                            retryCount, timeSpan.TotalSeconds);
                    });

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

                    await seeder.SeedAllAsync();
                });
                logger.LogInformation("User Management database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Fatal error during User Management database migration/seeding. Application might not function correctly.");
            }
        }
        else
        {
            logger.LogInformation(
                "User Management database migration and seeding skipped (Non-development environment).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
