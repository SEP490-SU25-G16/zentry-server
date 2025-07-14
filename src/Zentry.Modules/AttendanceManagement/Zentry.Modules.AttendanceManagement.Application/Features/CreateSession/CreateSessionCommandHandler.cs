using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    IScheduleService scheduleService, // Service để kiểm tra thông tin lịch trình
    IUserAttendanceService userAttendanceService, // Service để kiểm tra thông tin giảng viên
    IRedisService redisService, // Service để làm việc với Redis
    ILogger<CreateSessionCommandHandler> logger) // Để ghi log
    : ICommandHandler<CreateSessionCommand, CreateSessionResponse> // Thay IRequest bằng ICommandHandler nếu bạn dùng thư viện riêng của Zentry.
{
    public async Task<CreateSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        // --- 1. Kiểm tra điều kiện nghiệp vụ ---

        // 1.1. Kiểm tra giảng viên có tồn tại và có quyền
        var lecturer = await userAttendanceService.GetLecturerByIdAsync(request.UserId, cancellationToken);
        if (lecturer == null)
        {
            logger.LogWarning("CreateSession failed: Lecturer with ID {LecturerId} not found or not authorized.", request.UserId);
            throw new BusinessRuleException("LECTURER_NOT_FOUND_OR_UNAUTHORIZED", "Giảng viên không tồn tại hoặc không có quyền.");
        }

        // 1.2. Kiểm tra thông tin buổi học/lịch trình
        var schedule = await scheduleService.GetScheduleByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule == null || !schedule.IsActive)
        {
            logger.LogWarning("CreateSession failed: Schedule with ID {ScheduleId} not found or not active.", request.ScheduleId);
            throw new BusinessRuleException("SCHEDULE_INVALID", "Lịch trình buổi học không hợp lệ hoặc không hoạt động.");
        }

        // 1.3. Kiểm tra giảng viên được phân công cho buổi học
        if (schedule.LecturerId != request.UserId)
        {
            logger.LogWarning("CreateSession failed: Lecturer {LecturerId} is not assigned to schedule {ScheduleId}.", request.UserId, request.ScheduleId);
            throw new BusinessRuleException("LECTURER_NOT_ASSIGNED", "Giảng viên không được phân công cho buổi học này.");
        }

        // 1.4. Kiểm tra khung thời gian cho phép khởi tạo phiên
        var currentTime = DateTime.UtcNow; // Sử dụng UTC cho tính nhất quán
        var sessionAllowedStartTime = schedule.ScheduledStartTime.AddMinutes(-15); // 15 phút trước giờ bắt đầu
        var sessionAllowedEndTime = schedule.ScheduledEndTime.AddMinutes(15);    // 15 phút sau giờ kết thúc

        if (currentTime < sessionAllowedStartTime || currentTime > sessionAllowedEndTime)
        {
            logger.LogWarning("CreateSession failed: Current time {CurrentTime} is outside allowed window ({AllowedStart} - {AllowedEnd}) for schedule {ScheduleId}.",
                currentTime, sessionAllowedStartTime, sessionAllowedEndTime, request.ScheduleId);
            throw new BusinessRuleException("OUT_OF_TIME_WINDOW", $"Chưa đến hoặc đã quá thời gian cho phép khởi tạo phiên. Giờ hiện tại: {currentTime:HH:mm}, Thời gian cho phép: {sessionAllowedStartTime:HH:mm} - {sessionAllowedEndTime:HH:mm}.");
        }

        // 1.5. Kiểm tra chưa có phiên điểm danh nào đang hoạt động cho buổi này
        // Để làm điều này hiệu quả, chúng ta sẽ sử dụng Redis để kiểm tra nhanh.
        // Quy ước key: "active_session:{ScheduleId}"
        var activeSessionKey = $"active_session:{request.ScheduleId}";
        if (await redisService.KeyExistsAsync(activeSessionKey))
        {
            logger.LogWarning("CreateSession failed: An active session already exists for schedule {ScheduleId}.", request.ScheduleId);
            throw new BusinessRuleException("SESSION_ALREADY_ACTIVE", "Buổi học này đã có phiên điểm danh đang hoạt động.");
        }

        // --- 2. Tạo đối tượng Session ---
        var session = Session.Create(
            request.ScheduleId,
            request.UserId,
            request.StartTime, // Giả định StartTime/EndTime trong request khớp với ý định của giảng viên
            request.EndTime
        );

        // --- 3. Lưu Session vào database (lưu lâu dài) ---
        // SessionRepository có thể lưu vào MongoDB hoặc SQL DB cho lưu trữ lâu dài.
        await sessionRepository.AddAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        // --- 4. Lưu dữ liệu phiên vào Redis (tạm thời, cho thời gian thực) ---
        // Lưu ID phiên vào Redis với thời gian sống (TTL) phù hợp (ví dụ: 1 ngày như mô tả)
        // Key có thể là "active_session:{ScheduleId}" và value là SessionId hoặc một DTO của Session.
        await redisService.SetAsync(activeSessionKey, session.Id.ToString(), TimeSpan.FromDays(1)); // Lưu ID phiên vào key đại diện cho lịch trình

        // --- 5. Ghi log hành động khởi tạo phiên ---
        logger.LogInformation("Session {SessionId} created successfully for Schedule {ScheduleId} by Lecturer {LecturerId}. Stored in Redis with key {RedisKey}.",
            session.Id, session.ScheduleId, session.UserId, activeSessionKey);

        // --- 6. Gửi thông báo đến các máy trong lớp (nếu có NotificationService) ---
        // Bạn sẽ cần inject INotificationService và gọi nó ở đây.
        // Ví dụ: _notificationService.SendSessionStartedNotification(session.Id, session.ScheduleId, session.StartTime);
        logger.LogInformation("Sending session started notification for Session {SessionId}.", session.Id);


        // --- 7. Trả về Response ---
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
