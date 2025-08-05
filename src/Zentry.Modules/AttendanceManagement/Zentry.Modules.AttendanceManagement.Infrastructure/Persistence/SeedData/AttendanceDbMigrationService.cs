using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.SeedData;

public class AttendanceDbMigrationService(
    IServiceProvider serviceProvider,
    ILogger<AttendanceDbMigrationService> logger,
    IHostEnvironment env)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Chỉ chạy trong môi trường Development
        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "Attendance Management database migration and seeding initiated (Development environment).");

            // Retry policy giống như ScheduleDbMigrationService
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, // Thử lại 2 lần
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Back-off theo cấp số nhân
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception,
                            "Attendance Management DB migration/seed attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                            retryCount, timeSpan.TotalSeconds);
                    });

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var seeder =
                        scope.ServiceProvider.GetRequiredService<AttendanceDbSeeder>();

                    await seeder.SeedAllAsync(false, cancellationToken);
                });
                logger.LogInformation("Attendance Management database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Fatal error during Attendance Management database migration/seeding. Application might not function correctly.");
            }
        }
        else
        {
            logger.LogInformation(
                "Attendance Management database migration and seeding skipped (Non-development environment).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}