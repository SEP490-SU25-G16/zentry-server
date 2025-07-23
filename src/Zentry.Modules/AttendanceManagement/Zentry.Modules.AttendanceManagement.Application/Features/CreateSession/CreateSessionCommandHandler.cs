using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    IScheduleService scheduleService,
    IUserService userService,
    IConfigurationService configService,
    ILogger<CreateSessionCommandHandler> logger)
    : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    public async Task<CreateSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var lecturer =
            await userService.GetUserByIdAndRoleAsync("Lecturer", request.UserId, cancellationToken);
        if (lecturer == null)
        {
            logger.LogWarning("CreateSession failed: Lecturer with ID {LecturerId} not found or not authorized.",
                request.UserId);
            throw new BusinessRuleException("LECTURER_NOT_FOUND_OR_UNAUTHORIZED",
                "Giảng viên không tồn tại hoặc không có quyền.");
        }

        var schedule = await scheduleService.GetScheduleByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule is not { IsActive: true })
        {
            logger.LogWarning("CreateSession failed: Schedule with ID {ScheduleId} not found or not active.",
                request.ScheduleId);
            throw new BusinessRuleException("SCHEDULE_INVALID",
                "Lịch trình buổi học không hợp lệ hoặc không hoạt động.");
        }

        if (schedule.LecturerId != request.UserId)
        {
            logger.LogWarning("CreateSession failed: Lecturer {LecturerId} is not assigned to schedule {ScheduleId}.",
                request.UserId, request.ScheduleId);
            throw new BusinessRuleException("LECTURER_NOT_ASSIGNED",
                "Giảng viên không được phân công cho buổi học này.");
        }

        logger.LogInformation("Fetching all relevant settings for schedule {ScheduleId}.", request.ScheduleId);

        var globalSettingsTask =
            configService.GetAllSettingsForScopeAsync(AttendanceScopeTypes.Global, Guid.Empty, cancellationToken);
        var courseSettingsTask = Task.FromResult(new Dictionary<string, SettingContract>());
        if (schedule.CourseId != Guid.Empty)
            courseSettingsTask =
                configService.GetAllSettingsForScopeAsync(AttendanceScopeTypes.Course, schedule.CourseId,
                    cancellationToken);
        var sessionSettingsTask =
            configService.GetAllSettingsForScopeAsync(AttendanceScopeTypes.Session, request.ScheduleId,
                cancellationToken);

        await Task.WhenAll(globalSettingsTask, courseSettingsTask, sessionSettingsTask);

        var globalSettings = await globalSettingsTask;
        var courseSettings = await courseSettingsTask;
        var sessionSettings = await sessionSettingsTask;

        var allRelevantSettingsContracts = new List<SettingContract>();
        allRelevantSettingsContracts.AddRange(globalSettings.Values);
        allRelevantSettingsContracts.AddRange(courseSettings.Values);
        allRelevantSettingsContracts.AddRange(sessionSettings.Values);

        var finalConfigDictionary = allRelevantSettingsContracts
            .GroupBy(s => s.AttributeKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().Value ?? string.Empty,
                StringComparer.OrdinalIgnoreCase
            );

        var currentTime = DateTime.UtcNow;
        var currentDate = DateOnly.FromDateTime(currentTime);

        if (currentDate < schedule.ScheduledStartDate || currentDate > schedule.ScheduledEndDate)
        {
            logger.LogWarning(
                "CreateSession failed: Current date {CurrentDate} is outside the course period ({StartDate} - {EndDate}) for schedule {ScheduleId}.",
                currentDate, schedule.ScheduledStartDate, schedule.ScheduledEndDate, request.ScheduleId);
            throw new BusinessRuleException("OUT_OF_COURSE_PERIOD",
                $"Ngày hiện tại không nằm trong thời gian của khóa học. Khóa học từ {schedule.ScheduledStartDate:dd/MM/yyyy} đến {schedule.ScheduledEndDate:dd/MM/yyyy}.");
        }

        var todaySessionStartUnspecified = currentDate.ToDateTime(schedule.ScheduledStartTime);
        var todaySessionEndUnspecified = currentDate.ToDateTime(schedule.ScheduledEndTime);

        if (schedule.ScheduledEndTime < schedule.ScheduledStartTime)
            todaySessionEndUnspecified = todaySessionEndUnspecified.AddDays(1);

        var todaySessionStart = DateTime.SpecifyKind(todaySessionStartUnspecified, DateTimeKind.Utc);
        var todaySessionEnd = DateTime.SpecifyKind(todaySessionEndUnspecified, DateTimeKind.Utc);

        // Kiểm tra xem có session nào ACTIVE cho schedule này không (chỉ check trong DB, không dùng Redis)
        // Vì mục đích là tạo một session PENDING, không phải ACTIVE, nên việc này ít quan trọng hơn,
        // nhưng vẫn nên kiểm tra để tránh tạo nhiều session trùng lặp không cần thiết.
        // Bạn có thể thêm một phương thức trong ISessionRepository:
        // Task<bool> HasPendingOrActiveSessionForScheduleTodayAsync(Guid scheduleId, DateTime sessionDate, CancellationToken cancellationToken);
        // Hiện tại, tôi sẽ bỏ qua check Redis để đơn giản hóa như yêu cầu.
        // Để đơn giản hóa cho TEST, chúng ta sẽ bỏ qua bước check active_sessionKey trong Redis ở đây.
        // Việc này sẽ được xử lý trong StartSessionCommandHandler.

        var session = Session.Create(
            request.ScheduleId,
            request.UserId,
            todaySessionStart,
            todaySessionEnd,
            finalConfigDictionary
        );
        try
        {
            await sessionRepository.AddAsync(session, cancellationToken);
            await sessionRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Session {SessionId} created successfully for Schedule {ScheduleId} by Lecturer {LecturerId}.",
                session.Id, session.ScheduleId, session.UserId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save Session {SessionId} for Schedule {ScheduleId}.", session.Id,
                session.ScheduleId);
            throw new ApplicationException("An error occurred while creating the session.",
                e); // Throw generic app exception or rethrow
        }

        return new CreateSessionResponse
        {
            SessionId = session.Id,
            ScheduleId = session.ScheduleId,
            UserId = session.UserId,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            CreatedAt = session.CreatedAt,
            Status = session.Status
        };
    }
}