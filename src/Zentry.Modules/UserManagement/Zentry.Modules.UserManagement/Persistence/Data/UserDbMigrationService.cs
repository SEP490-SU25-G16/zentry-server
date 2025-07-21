using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Zentry.Modules.UserManagement.Persistence.Data;

public class UserDbMigrationService(
    IServiceProvider serviceProvider,
    ILogger<UserDbMigrationService> logger,
    IHostEnvironment env)
    : IHostedService
{
    // Inject IServiceProvider để tạo scope riêng, ILogger và IHostEnvironment để kiểm tra môi trường

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Chỉ chạy migration và seed trong môi trường phát triển
        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "User Management database migration and seeding initiated (Development environment).");

            var retryPolicy = Policy
                .Handle<Exception>() // Handle any exception during DB operations for retry
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
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
                    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>(); // Lấy DbSeeder từ scope

                    await seeder.SeedAllAsync(false); // Gọi SeedAllAsync
                });
                logger.LogInformation("User Management database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Fatal error during User Management database migration/seeding. Application might not function correctly.");
                // Optionally stop the application if migration/seeding is critical for startup
                // _serviceProvider.GetRequiredService<IHostApplicationLifetime>().StopApplication();
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