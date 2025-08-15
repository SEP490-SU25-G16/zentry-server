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

            // Ensure FaceEmbeddings table exists first (idempotent for runtime safety)
            var createEmbeddingsTableSql = @"
CREATE TABLE IF NOT EXISTS ""FaceEmbeddings"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""EncryptedEmbedding"" bytea NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_FaceEmbeddings"" PRIMARY KEY (""Id""),
    CONSTRAINT ""UQ_FaceEmbeddings_UserId"" UNIQUE (""UserId"")
);
CREATE INDEX IF NOT EXISTS ""IX_FaceEmbeddings_UserId"" ON ""FaceEmbeddings"" (""UserId"");
";
            await dbContext.Database.ExecuteSqlRawAsync(createEmbeddingsTableSql, cancellationToken);

            // Ensure FaceIdVerifyRequests table exists (idempotent for runtime safety)
            var createRequestsTableSql = @"
CREATE TABLE IF NOT EXISTS ""FaceIdVerifyRequests"" (
    ""Id"" uuid NOT NULL,
    ""RequestGroupId"" uuid NOT NULL,
    ""TargetUserId"" uuid NOT NULL,
    ""InitiatorUserId"" uuid NULL,
    ""SessionId"" uuid NULL,
    ""ClassSectionId"" uuid NULL,
    ""Threshold"" real NOT NULL DEFAULT 0.7,
    ""Status"" integer NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""ExpiresAt"" timestamp with time zone NOT NULL,
    ""CompletedAt"" timestamp with time zone NULL,
    ""Matched"" boolean NULL,
    ""Similarity"" real NULL,
    ""NotificationId"" text NULL,
    ""MetadataJson"" jsonb NULL,
    CONSTRAINT ""PK_FaceIdVerifyRequests"" PRIMARY KEY (""Id"")
);
CREATE INDEX IF NOT EXISTS ""IX_FaceIdReq_Group_Target_Status_Exp"" ON ""FaceIdVerifyRequests"" (""RequestGroupId"", ""TargetUserId"", ""Status"", ""ExpiresAt"");
CREATE INDEX IF NOT EXISTS ""IX_FaceIdReq_Session_Status"" ON ""FaceIdVerifyRequests"" (""SessionId"", ""Status"");
CREATE INDEX IF NOT EXISTS ""IX_FaceIdReq_ExpiresAt"" ON ""FaceIdVerifyRequests"" (""ExpiresAt"");
";
            await dbContext.Database.ExecuteSqlRawAsync(createRequestsTableSql, cancellationToken);

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}