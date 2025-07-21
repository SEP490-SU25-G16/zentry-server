using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Not directly used here, but in DbSeederExtensions
using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Enums;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services; // For IPasswordHasher

namespace Zentry.Modules.UserManagement.Persistence.Data;

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

            // Only MigrateAsync() is needed. It creates the database if it doesn't exist and applies all migrations.
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

    public async Task SeedNewDataAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>(); // Get from DI

        await UserSeedData.SeedAsync(context, passwordHasher, logger); // Pass passwordHasher
    }

    public async Task ReseedAllAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>(); // Get from DI

        await UserSeedData.ReseedAsync(context, passwordHasher, logger); // Pass passwordHasher
    }

    public async Task ClearAllDataAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        await UserSeedData.ClearAllData(context, logger);
    }

    public async Task<bool> HasDataAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        return await context.Accounts.AnyAsync() || await context.Users.AnyAsync();
    }
}
