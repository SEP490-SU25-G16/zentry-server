using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.StartSession;

public class StartSessionCommandHandler(
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IRedisService redisService,
    ISessionWhitelistRepository sessionWhitelistRepository,
    ILogger<StartSessionCommandHandler> logger)
    : ICommandHandler<StartSessionCommand, StartSessionResponse>
{
    public async Task<StartSessionResponse> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to start session {SessionId} by user {UserId}.", request.SessionId,
            request.UserId);

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning("StartSession failed: Session with ID {SessionId} not found.", request.SessionId);
            throw new NotFoundException(nameof(Session), request.SessionId);
        }

        if (session.LecturerId != request.UserId)
        {
            logger.LogWarning("StartSession failed: Lecturer {LecturerId} is not assigned to session {SessionId}.",
                request.UserId, request.SessionId);
            throw new BusinessRuleException("LECTURER_NOT_ASSIGNED", "Giảng viên không được phân công cho phiên này.");
        }

        // Kiểm tra trạng thái session
        if (!Equals(session.Status, SessionStatus.Pending))
        {
            logger.LogWarning(
                "StartSession failed: Session {SessionId} is not in Pending status. Current status: {Status}.",
                session.Id, session.Status);
            throw new BusinessRuleException("SESSION_NOT_PENDING", "Phiên điểm danh chưa ở trạng thái chờ kích hoạt.");
        }

        var sessionConfigSnapshot = session.SessionConfigs; // Sử dụng SessionConfigs đã lưu trong Session entity

        var attendanceWindowMinutes = sessionConfigSnapshot.AttendanceWindowMinutes;

        var currentTime = DateTime.UtcNow;
        var sessionAllowedStartTime = session.StartTime.AddMinutes(-attendanceWindowMinutes);
        var sessionAllowedEndTime = session.EndTime.AddMinutes(attendanceWindowMinutes);

        if (currentTime < sessionAllowedStartTime || currentTime > sessionAllowedEndTime)
        {
            logger.LogWarning(
                "StartSession failed: Current time {CurrentTime} is outside allowed window ({AllowedStart} - {AllowedEnd}) for session {SessionId}. Configured window: {ConfigWindow} minutes.",
                currentTime, sessionAllowedStartTime, sessionAllowedEndTime, request.SessionId,
                attendanceWindowMinutes);
            throw new BusinessRuleException("OUT_OF_TIME_WINDOW",
                $"Chưa đến hoặc đã quá thời gian cho phép khởi tạo phiên. Giờ hiện tại: {currentTime:HH:mm}, Thời gian cho phép: {sessionAllowedStartTime:HH:mm} - {sessionAllowedEndTime:HH:mm}.");
        }

        // Kiểm tra xem đã có SCHEDULE active chưa
        var activeScheduleKey = $"active_schedule:{session.ScheduleId}";
        if (await redisService.KeyExistsAsync(activeScheduleKey))
        {
            logger.LogWarning("StartSession failed: An active session already exists for schedule {ScheduleId}.",
                session.ScheduleId);
            throw new BusinessRuleException("SESSION_ALREADY_ACTIVE",
                "Buổi học này đã có phiên điểm danh đang hoạt động.");
        }

        // --- 1. Kích hoạt Session ---
        session.ActivateSession(); // Gọi phương thức mới trong Session entity
        await sessionRepository.UpdateAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Session {SessionId} status updated to Active.", session.Id);

        // --- 2. Kích hoạt Round đầu tiên ---
        var firstRound = (await roundRepository.GetRoundsBySessionIdAsync(session.Id, cancellationToken))
            .FirstOrDefault(r => r.RoundNumber == 1);
        if (firstRound is not null)
        {
            firstRound.UpdateStatus(RoundStatus.Active);
            await roundRepository.UpdateAsync(firstRound, cancellationToken);
            await roundRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("First round ({RoundId}) for Session {SessionId} status updated to Active.",
                firstRound.Id, session.Id);
        }
        else
        {
            logger.LogWarning("No first round found for Session {SessionId} to activate.", session.Id);
        }

        // --- 3. Tải Whitelist từ DocumentDB và cache vào Redis ---
        var sessionWhitelist = await sessionWhitelistRepository.GetBySessionIdAsync(session.Id, cancellationToken);
        if (sessionWhitelist != null)
        {
            var whitelistJson = JsonSerializer.Serialize(sessionWhitelist.WhitelistedDeviceIds);
            var totalSessionDuration = session.EndTime.Subtract(currentTime) // Thời gian còn lại của session
                .Add(TimeSpan.FromMinutes(session.AttendanceWindowMinutes * 2)); // Cộng thêm buffer

            // Cache whitelist vào Redis
            await redisService.SetAsync($"session_whitelist:{session.Id}", whitelistJson, totalSessionDuration);
            logger.LogInformation(
                "Whitelist for Session {SessionId} loaded from DocumentDB and cached in Redis. Total devices: {DeviceCount}.",
                session.Id, sessionWhitelist.WhitelistedDeviceIds.Count);
        }
        else
        {
            logger.LogWarning(
                "No whitelist found in DocumentDB for Session {SessionId}. An empty whitelist will be cached to prevent errors.",
                session.Id);
            // Cache một whitelist rỗng để đảm bảo GetSessionWhitelist không lỗi
            await redisService.SetAsync($"session_whitelist:{session.Id}", "[]",
                TimeSpan.FromMinutes(5)); // Cache rỗng trong 5 phút
        }

        // --- 4. Cập nhật cờ active_session trong Redis ---
        // TTL sẽ là tổng thời gian còn lại của session
        var totalRemainingDuration = session.EndTime.Subtract(currentTime)
            .Add(TimeSpan.FromMinutes(sessionConfigSnapshot.AttendanceWindowMinutes * 2));

        // --- 4. Cập nhật các cờ trạng thái trong Redis ---
        // ĐIỀU CHỈNH: Chỉ sử dụng SetAsync cho từng key

        // Key: Xác nhận rằng session này đang active (dùng cho SubmitScanDataCommandHandler)
        await redisService.SetAsync($"session:{session.Id}", SessionStatus.Active.ToString(), totalRemainingDuration);
        // Key: Đảm bảo chỉ có một session active cho ScheduleId cụ thể
        await redisService.SetAsync(activeScheduleKey, session.Id.ToString(), totalRemainingDuration);

        // --- 5. (Optional) Gửi thông báo đến các máy trong lớp ---
        logger.LogInformation("Sending session started notification for Session {SessionId}.", session.Id);
        // Có thể publish một MassTransit event ở đây, ví dụ: await publishEndpoint.Publish(new SessionStartedEvent(...), cancellationToken);

        // --- 6. Trả về Response ---
        return new StartSessionResponse
        {
            SessionId = session.Id,
            ScheduleId = session.ScheduleId,
            LecturerId = session.LecturerId,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            CreatedAt = session.CreatedAt,
            Status = session.Status
        };
    }
}
