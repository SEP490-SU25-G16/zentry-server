using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.Enums;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Contracts.Messages;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IScheduleService scheduleService,
    IUserService userService,
    IRedisService redisService,
    IConfigurationService configService,
    IBus bus,
    ILogger<CreateSessionCommandHandler> logger)
    : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    public async Task<CreateSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        // --- 1. Kiểm tra điều kiện nghiệp vụ ---
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

        var sessionConfigSnapshot = SessionConfigSnapshot.FromDictionary(finalConfigDictionary);

        var attendanceWindowMinutes = sessionConfigSnapshot.AttendanceWindowMinutes;
        var totalAttendanceRounds = sessionConfigSnapshot.TotalAttendanceRounds;
        var absentReportGracePeriodHours = sessionConfigSnapshot.AbsentReportGracePeriodHours;
        var manualAdjustmentGracePeriodHours = sessionConfigSnapshot.ManualAdjustmentGracePeriodHours;

        var currentTime = DateTime.UtcNow;

        var scheduledStart = schedule.ScheduledStartDate.ToDateTime(schedule.ScheduledStartTime);
        var scheduledEnd = schedule.ScheduledEndDate.ToDateTime(schedule.ScheduledEndTime);

        var sessionAllowedStartTime = scheduledStart.AddMinutes(-attendanceWindowMinutes);
        var sessionAllowedEndTime = scheduledEnd.AddMinutes(attendanceWindowMinutes);

        if (currentTime < sessionAllowedStartTime || currentTime > sessionAllowedEndTime)
        {
            logger.LogWarning(
                "CreateSession failed: Current time {CurrentTime} is outside allowed window ({AllowedStart} - {AllowedEnd}) for schedule {ScheduleId}. Configured window: {ConfigWindow} minutes.",
                currentTime, sessionAllowedStartTime, sessionAllowedEndTime, request.ScheduleId,
                attendanceWindowMinutes);
            throw new BusinessRuleException("OUT_OF_TIME_WINDOW",
                $"Chưa đến hoặc đã quá thời gian cho phép khởi tạo phiên. Giờ hiện tại: {currentTime:HH:mm}, Thời gian cho phép: {sessionAllowedStartTime:HH:mm} - {sessionAllowedEndTime:HH:mm}.");
        }

        var activeSessionKey = $"active_session:{request.ScheduleId}";
        if (await redisService.KeyExistsAsync(activeSessionKey))
        {
            logger.LogWarning("CreateSession failed: An active session already exists for schedule {ScheduleId}.",
                request.ScheduleId);
            throw new BusinessRuleException("SESSION_ALREADY_ACTIVE",
                "Buổi học này đã có phiên điểm danh đang hoạt động.");
        }

        // --- 3. Tạo đối tượng Session (Domain Entity) với thời gian từ Schedule ---
        var session = Session.Create(
            request.ScheduleId,
            request.UserId,
            scheduledStart,
            scheduledEnd,
            finalConfigDictionary
        );

        // --- 4. Lưu Session vào database (lưu lâu dài) ---
        await sessionRepository.AddAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        // --- 5. Lưu dữ liệu phiên vào Redis (tạm thời, cho thời gian thực) ---
        var totalSessionDuration = session.EndTime.Subtract(session.StartTime)
            .Add(TimeSpan.FromMinutes(sessionConfigSnapshot.AttendanceWindowMinutes * 2));
        await redisService.SetAsync(activeSessionKey, session.Id.ToString(), totalSessionDuration);

        // --- 6. Xử lý tạo Round đầu tiên và publish message cho các Round còn lại ---
        if (totalAttendanceRounds > 0)
        {
            var totalSessionTime = scheduledEnd - scheduledStart;
            var durationPerRoundSeconds = totalSessionTime.TotalSeconds / totalAttendanceRounds;

            var firstRoundEndTime = scheduledStart.AddSeconds(durationPerRoundSeconds);

            var firstRound = Round.Create(
                session.Id,
                1,
                scheduledStart,
                firstRoundEndTime
            );
            firstRound.UpdateStatus(RoundStatus.Active);
            await roundRepository.AddAsync(firstRound, cancellationToken);
            await roundRepository.SaveChangesAsync(cancellationToken);

            // Publish message for other rounds
            if (totalAttendanceRounds > 1)
            {
                var createRoundMessage = new CreateRoundMessage(
                    session.Id,
                    1,
                    totalAttendanceRounds,
                    scheduledStart,
                    scheduledEnd
                );
                await bus.Publish(createRoundMessage, cancellationToken);
            }
        }
        else
        {
            logger.LogWarning("totalAttendanceRounds is 0 for Session {SessionId}. No rounds will be created.",
                session.Id);
        }

        // --- 7. Ghi log hành động khởi tạo phiên ---
        logger.LogInformation(
            "Session {SessionId} created successfully for Schedule {ScheduleId} by Lecturer {LecturerId}. Stored in Redis with key {RedisKey}.",
            session.Id, session.ScheduleId, session.UserId, activeSessionKey);

        // --- 8. Gửi thông báo đến các máy trong lớp (nếu có NotificationService) ---
        logger.LogInformation("Sending session started notification for Session {SessionId}.", session.Id);

        // --- 9. Trả về Response ---
        return new CreateSessionResponse
        {
            SessionId = session.Id,
            ScheduleId = session.ScheduleId,
            UserId = session.UserId,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            CreatedAt = session.CreatedAt
        };
    }
}
