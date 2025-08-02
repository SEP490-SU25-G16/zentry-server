using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.EndSession;

public class EndSessionCommandHandler(
    ISessionRepository sessionRepository,
    IRedisService redisService,
    ILogger<EndSessionCommandHandler> logger)
    : ICommandHandler<EndSessionCommand, EndSessionResponse>
{
    public async Task<EndSessionResponse> Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to end session {SessionId} by user {UserId}.", request.SessionId,
            request.UserId);

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning("EndSession failed: Session with ID {SessionId} not found.", request.SessionId);
            throw new NotFoundException(nameof(EndSessionCommandHandler), request.SessionId);
        }

        // Kiểm tra giảng viên có quyền end session này không
        if (session.UserId != request.UserId)
        {
            logger.LogWarning("EndSession failed: Lecturer {LecturerId} is not assigned to session {SessionId}.",
                request.UserId, request.SessionId);
            throw new BusinessRuleException("LECTURER_NOT_ASSIGNED", "Giảng viên không được phân công cho phiên này.");
        }

        // Kiểm tra trạng thái session có phải là Active không
        if (!Equals(session.Status, SessionStatus.Active))
        {
            logger.LogWarning(
                "EndSession failed: Session {SessionId} is not in Active status. Current status: {Status}.",
                session.Id, session.Status);
            throw new BusinessRuleException("SESSION_NOT_ACTIVE", "Phiên điểm danh chưa ở trạng thái hoạt động.");
        }

        // --- 1. Kết thúc Session trong DB ---
        session.CompleteSession(); // Gọi phương thức mới trong Session entity
        await sessionRepository.UpdateAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Session {SessionId} status updated to Completed.", session.Id);

        // --- 2. Xóa các cờ trạng thái trong Redis để ngăn chặn các request sau đó ---
        var activeScheduleKey = $"active_schedule:{session.ScheduleId}";
        await redisService.RemoveAsync($"session:{session.Id}");
        await redisService.RemoveAsync(activeScheduleKey);

        logger.LogInformation("Redis keys for Session {SessionId} and Schedule {ScheduleId} deleted.",
            session.Id, session.ScheduleId);

        // --- 3. (Optional) Gửi thông báo đến các máy trong lớp ---
        logger.LogInformation("Sending session ended notification for Session {SessionId}.", session.Id);
        // Có thể publish một MassTransit event ở đây, ví dụ: await publishEndpoint.Publish(new SessionEndedEvent(...), cancellationToken);

        // --- 4. Trả về Response ---
        return new EndSessionResponse
        {
            SessionId = session.Id,
            Status = session.Status,
            EndTime = session.EndTime,
            UpdatedAt = session.UpdatedAt
        };
    }
}
