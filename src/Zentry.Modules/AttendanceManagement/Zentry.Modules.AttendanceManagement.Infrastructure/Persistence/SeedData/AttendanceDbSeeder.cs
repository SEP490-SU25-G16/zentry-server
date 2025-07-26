using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.SharedKernel.Contracts.Schedule;

// Cần để dùng GetSchedulesAndClassSectionsForAttendanceSeedQuery

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.SeedData;

public class AttendanceDbSeeder(
    IServiceProvider serviceProvider,
    ILogger<AttendanceDbSeeder> logger,
    IMediator mediator)
{
    public async Task SeedAllAsync(bool recreateDatabase = false, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var attendanceContext = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();

        try
        {
            logger.LogInformation("Starting database seeding process for Attendance Management module...");
            if (recreateDatabase)
            {
                logger.LogWarning("Recreating database for Attendance Management module...");
                await attendanceContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            logger.LogInformation("Applying pending migrations for Attendance Management module...");
            await attendanceContext.Database.MigrateAsync(cancellationToken);

            // --- Bắt đầu phần lấy dữ liệu từ Schedule Module qua Mediator ---
            logger.LogInformation("Requesting Schedule and ClassSection data from Schedule module via Mediator...");

            var scheduleDataResponse = await mediator.Send(new GetSchedulesAndClassSectionsForAttendanceSeedQuery(),
                cancellationToken);

            if (scheduleDataResponse != null && scheduleDataResponse.Schedules.Any())
            {
                logger.LogInformation(
                    $"Received {scheduleDataResponse.Schedules.Count} Schedule DTOs and {scheduleDataResponse.ClassSections.Count} ClassSection DTOs.");

                await AttendanceSeedData.SeedSessionsAndRoundsAsync(
                    attendanceContext,
                    scheduleDataResponse.Schedules,
                    scheduleDataResponse.ClassSections,
                    logger,
                    cancellationToken
                );
            }
            else
            {
                logger.LogWarning(
                    "No schedule data received from Schedule module. Skipping Session and Round seeding for Attendance module.");
            }

            // Attendance Record sẽ được seed sau khi bạn cung cấp định nghĩa Entity của nó

            await attendanceContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Attendance Management module data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Attendance Management module database");
            throw;
        }
    }

    public async Task ClearAllData(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();

        logger.LogInformation("Clearing all Attendance Management data...");

        context.AttendanceRecords.RemoveRange(context.AttendanceRecords);
        context.Rounds.RemoveRange(context.Rounds);
        context.Sessions.RemoveRange(context.Sessions);
        context.UserRequests.RemoveRange(context.UserRequests); // Nếu UserRequest cũng là dữ liệu seed

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("All Attendance Management data cleared.");
    }

    public async Task ReseedAsync(bool recreateDatabase = false, CancellationToken cancellationToken = default)
    {
        await ClearAllData(cancellationToken);
        await SeedAllAsync(recreateDatabase, cancellationToken);
    }
}