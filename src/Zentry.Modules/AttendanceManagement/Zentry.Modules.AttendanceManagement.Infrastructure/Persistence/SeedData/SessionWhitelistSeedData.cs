using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.SeedData;

public static class SessionWhitelistSeedData
{
    private static List<SessionWhitelist> SeededSessionWhitelists { get; set; } = [];

    public static async Task SeedSessionWhitelistsAsync(
        AttendanceDbContext context,
        IScanLogWhitelistRepository whitelistRepository,
        IMediator mediator,
        List<SeededScheduleDto> seededScheduleDtos,
        List<SeededClassSectionDto> seededClassSectionDtos,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting SessionWhitelist seed data...");

            if (!seededScheduleDtos.Any())
            {
                logger?.LogWarning(
                    "No Schedule DTOs provided to create SessionWhitelists. Skipping SessionWhitelist seeding.");
                return;
            }

            if (await whitelistRepository.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("SessionWhitelist data already exists. Skipping SessionWhitelist seed.");
                SeededSessionWhitelists = await whitelistRepository.GetAllAsync(cancellationToken);
                return;
            }

            // Lấy tất cả sessions đã được seed
            var existingSessions = await context.Sessions.AsNoTracking().ToListAsync(cancellationToken);
            if (!existingSessions.Any())
            {
                logger?.LogWarning("No Sessions found in database. Cannot create SessionWhitelists without Sessions.");
                return;
            }

            logger?.LogInformation($"Found {existingSessions.Count} existing sessions to create whitelists for.");

            // Lấy danh sách tất cả sinh viên active từ User module
            logger?.LogInformation("Fetching all active students from User module...");
            var getUsersByRoleQuery = new GetUsersByRoleIntegrationQuery(Role.Student);
            var allStudentsResponse = await mediator.Send(getUsersByRoleQuery, cancellationToken);
            var allStudentIds = allStudentsResponse.UserIds;

            if (!allStudentIds.Any())
                logger?.LogWarning(
                    "No active students found in User module. SessionWhitelists will only contain lecturer devices.");
            else
                logger?.LogInformation($"Found {allStudentIds.Count} active students in User module.");

            // Lấy device mapping cho tất cả sinh viên
            Dictionary<Guid, Guid> studentDeviceMap = new();
            if (allStudentIds.Any())
                try
                {
                    logger?.LogInformation("Fetching device mappings for all students...");
                    var getStudentDevicesQuery = new GetDevicesByUsersIntegrationQuery(allStudentIds);
                    var studentDevicesResponse = await mediator.Send(getStudentDevicesQuery, cancellationToken);
                    studentDeviceMap = studentDevicesResponse.UserDeviceMap;
                    logger?.LogInformation(
                        $"Successfully retrieved device mappings for {studentDeviceMap.Count} students.");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error fetching student device mappings. Continuing with empty device map.");
                    studentDeviceMap = new Dictionary<Guid, Guid>();
                }

            var whitelistsToAdd = new List<SessionWhitelist>();
            var processedCount = 0;

            foreach (var session in existingSessions)
                try
                {
                    // Tìm schedule và classsection tương ứng
                    var schedule = seededScheduleDtos.FirstOrDefault(s => s.Id == session.ScheduleId);
                    if (schedule == null)
                    {
                        logger?.LogWarning(
                            $"Schedule not found for session {session.Id}. Skipping whitelist creation.");
                        continue;
                    }

                    var classSection = seededClassSectionDtos.FirstOrDefault(cs => cs.Id == schedule.ClassSectionId);
                    if (classSection == null)
                    {
                        logger?.LogWarning(
                            $"ClassSection not found for schedule {schedule.Id}. Skipping whitelist creation.");
                        continue;
                    }

                    var whitelistedDeviceIds = new HashSet<Guid>();

                    // 1. Thêm device của lecturer
                    try
                    {
                        var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(classSection.LecturerId);
                        var lecturerDeviceResponse = await mediator.Send(getLecturerDeviceQuery, cancellationToken);
                        whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId);
                        logger?.LogDebug(
                            $"Added lecturer device {lecturerDeviceResponse.DeviceId} for session {session.Id}");
                    }
                    catch (NotFoundException)
                    {
                        logger?.LogWarning(
                            $"Lecturer {classSection.LecturerId} does not have an active device. Skipping lecturer device for session {session.Id}.");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex,
                            $"Error getting lecturer device for session {session.Id}. Continuing without lecturer device.");
                    }

                    // 2. Thêm devices của sinh viên trong class section
                    try
                    {
                        var getStudentIdsQuery =
                            new GetStudentIdsByClassSectionIdIntegrationQuery(schedule.ClassSectionId);
                        var studentIdsResponse = await mediator.Send(getStudentIdsQuery, cancellationToken);
                        var enrolledStudentIds = studentIdsResponse.StudentIds;

                        if (enrolledStudentIds.Any())
                        {
                            // Lọc ra những sinh viên có device từ mapping đã lấy trước đó
                            var enrolledStudentDevices = enrolledStudentIds
                                .Where(studentId => studentDeviceMap.ContainsKey(studentId))
                                .Select(studentId => studentDeviceMap[studentId])
                                .ToList();

                            foreach (var deviceId in enrolledStudentDevices) whitelistedDeviceIds.Add(deviceId);

                            logger?.LogDebug(
                                $"Added {enrolledStudentDevices.Count} student devices for session {session.Id} (from {enrolledStudentIds.Count} enrolled students)");
                        }
                        else
                        {
                            logger?.LogDebug(
                                $"No enrolled students found for ClassSection {schedule.ClassSectionId} of session {session.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex,
                            $"Error getting enrolled students for session {session.Id}. Continuing without student devices.");
                    }

                    // 3. Tạo SessionWhitelist
                    var finalWhitelistedDevices = whitelistedDeviceIds.ToList();
                    var sessionWhitelist = SessionWhitelist.Create(session.Id, finalWhitelistedDevices);
                    whitelistsToAdd.Add(sessionWhitelist);

                    processedCount++;
                    if (processedCount % 50 == 0)
                        logger?.LogInformation(
                            $"Processed {processedCount}/{existingSessions.Count} sessions for whitelist creation...");

                    logger?.LogDebug(
                        $"Created whitelist for session {session.Id} with {finalWhitelistedDevices.Count} devices");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"Error creating whitelist for session {session.Id}. Skipping this session.");
                }

            // Lưu tất cả SessionWhitelists
            if (whitelistsToAdd.Any())
            {
                await whitelistRepository.AddRangeAsync(whitelistsToAdd, cancellationToken);
                logger?.LogInformation($"Added {whitelistsToAdd.Count} SessionWhitelists.");
                SeededSessionWhitelists = whitelistsToAdd;

                // Log thống kê
                var totalDevices = whitelistsToAdd.Sum(w => w.WhitelistedDeviceIds.Count);
                var avgDevicesPerWhitelist = whitelistsToAdd.Any() ? totalDevices / (double)whitelistsToAdd.Count : 0;

                logger?.LogInformation("SessionWhitelist seeding statistics:");
                logger?.LogInformation($"  - Total whitelists created: {whitelistsToAdd.Count}");
                logger?.LogInformation($"  - Total devices across all whitelists: {totalDevices}");
                logger?.LogInformation($"  - Average devices per whitelist: {avgDevicesPerWhitelist:F2}");
            }
            else
            {
                logger?.LogWarning("No SessionWhitelists were generated during seeding.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding SessionWhitelist data");
            throw;
        }
    }

    // Method để lấy thông tin session whitelists đã seed (nếu cần thiết)
    public static List<SessionWhitelist> GetSeededSessionWhitelists()
    {
        return SeededSessionWhitelists.ToList();
    }
}