using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.SeedData;

public class ScheduleDbSeeder(IServiceProvider serviceProvider, ILogger<ScheduleDbSeeder> logger)
{
    public async Task SeedAllAsync(bool recreateDatabase = false, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var scheduleContext = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            logger.LogInformation("Starting database seeding process for Schedule Management module...");
            if (recreateDatabase)
            {
                logger.LogWarning("Recreating database for Schedule Management module...");
                await scheduleContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            logger.LogInformation("Applying pending migrations for Schedule Management module...");
            await scheduleContext.Database.MigrateAsync(cancellationToken);

            // Seed data for independent entities
            await ScheduleSeedData.SeedCoursesAsync(scheduleContext, logger, cancellationToken);
            await ScheduleSeedData.SeedRoomsAsync(scheduleContext, logger, cancellationToken);

            // Get Lecturer IDs (User IDs) via Integration Query
            var lecturerResponse =
                await mediator.Send(new GetUsersByRoleIntegrationQuery(Role.Lecturer), cancellationToken);
            var lecturerUserIds = lecturerResponse.UserIds;

            if (lecturerUserIds.Count == 0)
                logger.LogWarning(
                    "No active Lecturers (User IDs) found via User Management Integration Query. Skipping ClassSection seeding.");
            else
                // Seed ClassSections (Needs Course and Lecturer User IDs)
                await ScheduleSeedData.SeedClassSectionsAsync(scheduleContext, lecturerUserIds, logger,
                    cancellationToken);

            // Seed Schedules (depends on ClassSections and Rooms)
            await ScheduleSeedData.SeedSchedulesAsync(scheduleContext, logger, cancellationToken);

            // Get Student IDs (User IDs) via Integration Query
            var studentIdsResponse =
                await mediator.Send(new GetUsersByRoleIntegrationQuery(Role.Student), cancellationToken);
            await ScheduleSeedData.SeedEnrollmentsAsync(scheduleContext, studentIdsResponse.UserIds, logger,
                cancellationToken);

            // Save all changes for ScheduleDbContext
            await scheduleContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Schedule Management module data seeding completed successfully.");

            // **KHÔNG GỌI Attendance module từ đây nữa.**
            // Attendance module sẽ tự khởi tạo yêu cầu dữ liệu khi nó seed.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Schedule Management module database");
            throw;
        }
    }

    public async Task ClearAllData(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        logger.LogInformation("Clearing all Schedule Management data...");
        context.Enrollments.RemoveRange(context.Enrollments);
        context.Schedules.RemoveRange(context.Schedules);
        context.ClassSections.RemoveRange(context.ClassSections);
        context.Rooms.RemoveRange(context.Rooms);
        context.Courses.RemoveRange(context.Courses);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("All Schedule Management data cleared.");
    }

    public async Task ReseedAsync(CancellationToken cancellationToken = default)
    {
        await ClearAllData(cancellationToken);
        await SeedAllAsync(false, cancellationToken);
    }
}