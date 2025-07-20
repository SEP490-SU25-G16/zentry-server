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
        // --- 1. Kiểm tra điều kiện nghiệp vụ (Không thay đổi) ---

        // 1.1. Kiểm tra giảng viên có tồn tại và có quyền
        var lecturer =
            await userService.GetUserByIdAndRoleAsync("Lecturer", request.UserId, cancellationToken);
        if (lecturer == null)
        {
            logger.LogWarning("CreateSession failed: Lecturer with ID {LecturerId} not found or not authorized.",
                request.UserId);
            throw new BusinessRuleException("LECTURER_NOT_FOUND_OR_UNAUTHORIZED",
                "Giảng viên không tồn tại hoặc không có quyền.");
        }

        // 1.2. Kiểm tra thông tin buổi học/lịch trình
        var schedule = await scheduleService.GetScheduleByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule is not { IsActive: true })
        {
            logger.LogWarning("CreateSession failed: Schedule with ID {ScheduleId} not found or not active.",
                request.ScheduleId);
            throw new BusinessRuleException("SCHEDULE_INVALID",
                "Lịch trình buổi học không hợp lệ hoặc không hoạt động.");
        }

        // 1.3. Kiểm tra giảng viên được phân công cho buổi học
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

        // --- 2. Tạo SessionConfigSnapshot Value Object từ dictionary đã hợp nhất ---
        var sessionConfigSnapshot = SessionConfigSnapshot.FromDictionary(finalConfigDictionary);

        // Lấy các giá trị cụ thể từ snapshot
        var attendanceWindowMinutes = sessionConfigSnapshot.AttendanceWindowMinutes;
        var totalAttendanceRounds = sessionConfigSnapshot.TotalAttendanceRounds; // Lấy giá trị này
        var absentReportGracePeriodHours = sessionConfigSnapshot.AbsentReportGracePeriodHours;
        var manualAdjustmentGracePeriodHours = sessionConfigSnapshot.ManualAdjustmentGracePeriodHours;


        // 1.4. Kiểm tra khung thời gian cho phép khởi tạo phiên
        var currentTime = DateTime.UtcNow;
        var sessionAllowedStartTime = schedule.ScheduledStartTime.AddMinutes(-attendanceWindowMinutes);
        var sessionAllowedEndTime = schedule.ScheduledEndTime.AddMinutes(attendanceWindowMinutes);

        if (currentTime < sessionAllowedStartTime || currentTime > sessionAllowedEndTime)
        {
            logger.LogWarning(
                "CreateSession failed: Current time {CurrentTime} is outside allowed window ({AllowedStart} - {AllowedEnd}) for schedule {ScheduleId}. Configured window: {ConfigWindow} minutes.",
                currentTime, sessionAllowedStartTime, sessionAllowedEndTime, request.ScheduleId,
                attendanceWindowMinutes);
            throw new BusinessRuleException("OUT_OF_TIME_WINDOW",
                $"Chưa đến hoặc đã quá thời gian cho phép khởi tạo phiên. Giờ hiện tại: {currentTime:HH:mm}, Thời gian cho phép: {sessionAllowedStartTime:HH:mm} - {sessionAllowedEndTime:HH:mm}.");
        }

        // 1.5. Kiểm tra chưa có phiên điểm danh nào đang hoạt động cho buổi này
        var activeSessionKey = $"active_session:{request.ScheduleId}";
        if (await redisService.KeyExistsAsync(activeSessionKey))
        {
            logger.LogWarning("CreateSession failed: An active session already exists for schedule {ScheduleId}.",
                request.ScheduleId);
            throw new BusinessRuleException("SESSION_ALREADY_ACTIVE",
                "Buổi học này đã có phiên điểm danh đang hoạt động.");
        }

        // --- 3. Tạo đối tượng Session (Domain Entity) với SessionConfigSnapshot ---
        var session = Session.Create(
            request.ScheduleId,
            request.UserId,
            request.StartTime,
            request.EndTime,
            finalConfigDictionary
        );

        // --- 4. Lưu Session vào database (lưu lâu dài) ---
        await sessionRepository.AddAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        // --- 5. Lưu dữ liệu phiên vào Redis (tạm thời, cho thời gian thực) ---
        var totalSessionDuration = session.EndTime.Subtract(session.StartTime)
            .Add(TimeSpan.FromMinutes(sessionConfigSnapshot.AttendanceWindowMinutes * 2));
        await redisService.SetAsync(activeSessionKey, session.Id.ToString(), totalSessionDuration);
        await redisService.SetAsync($"session:{session.Id}", JsonSerializer.Serialize(session),
            totalSessionDuration);

        // --- 6. Xử lý tạo Round đầu tiên và publish message cho các Round còn lại ---
        if (totalAttendanceRounds > 0)
        {
            var firstRound = Round.Create(
                session.Id,
                1,
                DateTime.UtcNow
            );
            firstRound.UpdateStatus(RoundStatus.Active);
            await roundRepository.AddAsync(firstRound, cancellationToken);
            await roundRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "First round (Round {RoundNumber}) created for Session {SessionId}.",
                firstRound.RoundNumber, session.Id);

            if (totalAttendanceRounds > 1)
            {
                logger.LogInformation(
                    "Publishing CreateRoundMessage for remaining rounds ({RemainingRounds}) in Session {SessionId}.",
                    totalAttendanceRounds - 1, session.Id);

                var createRoundMessage = new CreateRoundMessage(
                    session.Id,
                    totalAttendanceRounds,
                    1,
                    session.StartTime,
                    session.EndTime
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
