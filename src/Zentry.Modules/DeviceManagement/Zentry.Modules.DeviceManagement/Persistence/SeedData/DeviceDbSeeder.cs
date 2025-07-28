using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.DeviceManagement.Persistence;
using Zentry.Modules.DeviceManagement.Persistence.SeedData;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.DeviceManagement.Persistence.SeedData;

public class DeviceDbSeeder(IServiceProvider serviceProvider, ILogger<DeviceDbSeeder> logger)
{
    public async Task SeedAllAsync(bool recreateDatabase = false, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var deviceContext = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            logger.LogInformation("Starting database seeding process for Device Management module...");

            if (recreateDatabase)
            {
                logger.LogWarning("Recreating database for Device Management module...");
                await deviceContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            logger.LogInformation("Applying pending migrations for Device Management module...");
            await deviceContext.Database.MigrateAsync(cancellationToken);

            // Get User IDs from all roles via Integration Query
            var allUserIds = new List<Guid>();

            // Get Admin and Manager User IDs
            var adminResponse = await mediator.Send(new GetUsersByRoleIntegrationQuery(Role.Admin), cancellationToken);
            var managerResponse = await mediator.Send(new GetUsersByRoleIntegrationQuery(Role.Manager), cancellationToken);
            var lecturerResponse = await mediator.Send(new GetUsersByRoleIntegrationQuery(Role.Lecturer), cancellationToken);
            var studentResponse = await mediator.Send(new GetUsersByRoleIntegrationQuery(Role.Student), cancellationToken);

            allUserIds.AddRange(adminResponse.UserIds);
            allUserIds.AddRange(managerResponse.UserIds);
            allUserIds.AddRange(lecturerResponse.UserIds);
            allUserIds.AddRange(studentResponse.UserIds);

            if (allUserIds.Count == 0)
            {
                logger.LogWarning("No User IDs found via User Management Integration Query. Skipping Device seeding.");
                return;
            }

            logger.LogInformation($"Found {allUserIds.Count} users to create devices for.");

            // Seed Devices for all users
            await DeviceSeedData.SeedAsync(deviceContext, allUserIds, logger);

            // Save all changes
            await deviceContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Device Management module data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Device Management module database");
            throw;
        }
    }

    public async Task ClearAllData(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();

        await DeviceSeedData.ClearAllData(context, logger);
    }

    public async Task ReseedAsync(CancellationToken cancellationToken = default)
    {
        await ClearAllData(cancellationToken);
        await SeedAllAsync(false, cancellationToken);
    }
}
