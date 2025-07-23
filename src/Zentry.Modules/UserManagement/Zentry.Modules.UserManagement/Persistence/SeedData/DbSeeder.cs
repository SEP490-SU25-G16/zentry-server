using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services;

namespace Zentry.Modules.UserManagement.Persistence.SeedData;

public class DbSeeder(IServiceProvider serviceProvider, ILogger<DbSeeder> logger)
{
    public async Task SeedAllAsync(bool recreateDatabase = false)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>(); // Get from DI

        try
        {
            logger.LogInformation("Starting database seeding process for User Management module...");
            if (recreateDatabase)
            {
                logger.LogWarning("Recreating database for User Management module...");
                await context.Database.EnsureDeletedAsync();
            }

            logger.LogInformation("Applying pending migrations for User Management module...");
            await context.Database.MigrateAsync();

            // Seed data
            await UserSeedData.SeedAsync(context, passwordHasher, logger); // Pass passwordHasher

            logger.LogInformation("User Management module data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding User Management module database");
            throw;
        }
    }
}