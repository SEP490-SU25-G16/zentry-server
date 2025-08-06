using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zentry.Modules.FaceId.Persistence;

public class FaceIdDbMigrationService(IServiceProvider serviceProvider, ILogger<FaceIdDbMigrationService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FaceIdDbContext>();

            logger.LogInformation("Ensuring pgvector extension is enabled...");

            // Ensure pgvector extension is created
            await dbContext.Database.ExecuteSqlRawAsync(
                "CREATE EXTENSION IF NOT EXISTS vector;",
                cancellationToken);

            logger.LogInformation("Applying migrations for FaceId module...");
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("FaceId module migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations for FaceId module");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
