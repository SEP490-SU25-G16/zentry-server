using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class CreateSessionConsumer(
    ILogger<CreateSessionConsumer> logger,
    IServiceScopeFactory serviceScopeFactory,
    IBus bus)
    : IConsumer<CreateSesssionMessage>
{
    public async Task Consume(ConsumeContext<CreateSesssionMessage> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received ScheduleCreatedEvent for ScheduleId: {ScheduleId}, WeekDay: {WeekDay}.",
            message.ScheduleId, message.WeekDay);

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
            var configService =
                scope.ServiceProvider.GetRequiredService<IConfigurationService>(); // Đây là ConfigurationService mới

            logger.LogInformation("Fetching all relevant settings for schedule {ScheduleId}.", message.ScheduleId);

            var requests = new List<ScopeQueryRequest>
            {
                new(AttendanceScopeTypes.Global, Guid.Empty),
                new(AttendanceScopeTypes.Session, message.ScheduleId)
            };
            if (message.CourseId != Guid.Empty)
                requests.Add(new ScopeQueryRequest(AttendanceScopeTypes.Course, message.CourseId));

            var allSettingsResponse =
                await configService.GetMultipleSettingsInBatchAsync(requests, context.CancellationToken);

            var globalSettings = allSettingsResponse.SettingsByScopeType
                .GetValueOrDefault(AttendanceScopeTypes.Global, new List<SettingContract>())
                .ToDictionary(s => s.AttributeKey, s => s.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var courseSettings = allSettingsResponse.SettingsByScopeType
                .GetValueOrDefault(AttendanceScopeTypes.Course, new List<SettingContract>())
                .ToDictionary(s => s.AttributeKey, s => s.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var sessionSettings = allSettingsResponse.SettingsByScopeType
                .GetValueOrDefault(AttendanceScopeTypes.Session, new List<SettingContract>())
                .ToDictionary(s => s.AttributeKey, s => s.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var finalConfigDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var setting in globalSettings) finalConfigDictionary[setting.Key] = setting.Value;

            // Thêm Course settings (ghi đè Global)
            foreach (var setting in courseSettings) finalConfigDictionary[setting.Key] = setting.Value;

            // Thêm Session settings (ghi đè Global và Course)
            foreach (var setting in sessionSettings) finalConfigDictionary[setting.Key] = setting.Value;

            var sessionConfigSnapshot = SessionConfigSnapshot.FromDictionary(finalConfigDictionary);

            // Các giá trị cấu hình liên quan đến thời gian và rounds
            var attendanceWindowMinutes = sessionConfigSnapshot.AttendanceWindowMinutes;
            var totalAttendanceRounds =
                sessionConfigSnapshot.TotalAttendanceRounds; // Giữ lại để publish message tạo round

            var currentTime = DateTime.UtcNow;
            var currentDate = DateOnly.FromDateTime(currentTime);

            DayOfWeek systemDayOfWeek;
            try
            {
                systemDayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), message.WeekDay, true);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex,
                    "Invalid WeekDay string '{WeekDay}' received for ScheduleId {ScheduleId}. Cannot parse to System.DayOfWeek.",
                    message.WeekDay, message.ScheduleId);
                throw new BusinessRuleException("INVALID_WEEKDAY_FORMAT", $"Invalid WeekDay string: {message.WeekDay}");
            }

            var sessionsToPersist = new List<Session>();

            // --- 2. Duyệt qua các ngày trong khoảng Schedule để tạo Session (PENDING) ---
            // Áp dụng logic kiểm tra ngày và giờ tương tự CreateSessionCommandHandler
            for (var date = message.ScheduledStartDate; date <= message.ScheduledEndDate; date = date.AddDays(1))
                if (date.DayOfWeek == systemDayOfWeek)
                {
                    // Kiểm tra xem ngày hiện tại có nằm trong khoảng thời gian của khóa học không (đã làm ở trên, nhưng giữ lại logic)
                    if (date < message.ScheduledStartDate || date > message.ScheduledEndDate)
                    {
                        // Logic này thực ra đã được bao phủ bởi vòng lặp for, nhưng giữ lại để khớp
                        logger.LogWarning(
                            "ScheduleCreatedEventConsumer: Date {Date} is outside the course period ({StartDate} - {EndDate}) for schedule {ScheduleId}.",
                            date, message.ScheduledStartDate, message.ScheduledEndDate, message.ScheduleId);
                        // Không throw, chỉ bỏ qua ngày này nếu có lỗi logic
                        continue;
                    }

                    // Tạo thời điểm bắt đầu và kết thúc của buổi học cho NGÀY NÀY (UTC)
                    var todaySessionStartUnspecified = date.ToDateTime(message.ScheduledStartTime);
                    var todaySessionEndUnspecified = date.ToDateTime(message.ScheduledEndTime);

                    // Nếu session kéo dài qua ngày (ví dụ: 23:00 - 01:00)
                    if (message.ScheduledEndTime < message.ScheduledStartTime)
                        todaySessionEndUnspecified = todaySessionEndUnspecified.AddDays(1);

                    var todaySessionStart = DateTime.SpecifyKind(todaySessionStartUnspecified, DateTimeKind.Utc);
                    var todaySessionEnd = DateTime.SpecifyKind(todaySessionEndUnspecified, DateTimeKind.Utc);

                    // Tạo Session với trạng thái Pending
                    var session = Session.Create(
                        message.ScheduleId,
                        message.LecturerId,
                        todaySessionStart,
                        todaySessionEnd,
                        finalConfigDictionary
                    );
                    sessionsToPersist.Add(session);

                    logger.LogInformation("Prepared Session {SessionId} for Schedule {ScheduleId} on {SessionDate}.",
                        session.Id, message.ScheduleId, date.ToShortDateString());
                }

            // --- 3. LƯU TẤT CẢ SESSIONS VÀO DATABASE ---
            if (sessionsToPersist.Count > 0)
            {
                await sessionRepository.AddRangeAsync(sessionsToPersist, context.CancellationToken);
                await sessionRepository.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation(
                    "Successfully created and saved {NumSessions} sessions for Schedule {ScheduleId}.",
                    sessionsToPersist.Count, message.ScheduleId);

                // --- 4. Publish messages cho các Consumer khác (Tạo Rounds & Whitelist) ---
                foreach (var session in sessionsToPersist)
                {
                    var currentSessionConfigSnapshot = session.SessionConfigs;

                    if (currentSessionConfigSnapshot.TotalAttendanceRounds > 0)
                    {
                        var createRoundsMessage = new CreateRoundsMessage(
                            session.Id,
                            currentSessionConfigSnapshot.TotalAttendanceRounds,
                            session.StartTime,
                            session.EndTime
                        );
                        await bus.Publish(createRoundsMessage, context.CancellationToken);
                        logger.LogInformation("Published CreateSessionRoundsMessage for SessionId: {SessionId}.",
                            session.Id);
                    }
                    else
                    {
                        logger.LogInformation(
                            "No rounds will be created for Session {SessionId} as TotalAttendanceRounds is 0.",
                            session.Id);
                    }

                    // Publish message để tạo/tính toán Whitelist cho Session này
                    // Dữ liệu từ ScheduleCreatedEvent message gốc được truyền vào
                    var generateWhitelistMessage = new GenerateSessionWhitelistMessage(
                        session.Id,
                        message.ScheduleId,
                        message.LecturerId,
                        message.ClassSectionId
                    );
                    await bus.Publish(generateWhitelistMessage, context.CancellationToken);
                    logger.LogInformation("Published GenerateSessionWhitelistMessage for SessionId: {SessionId}.",
                        session.Id);
                }
            }
            else
            {
                logger.LogInformation("No sessions to create for Schedule {ScheduleId}.", message.ScheduleId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error processing ScheduleCreatedEvent for ScheduleId {ScheduleId}.",
                message.ScheduleId);
            throw;
        }
    }
}
