using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.EndSession;

public class EndSessionCommandHandler(
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IPublishEndpoint publishEndpoint,
    IRedisService redisService,
    ILogger<EndSessionCommandHandler> logger,
    IAttendanceCalculationService attendanceCalculationService,
    IAttendancePersistenceService attendancePersistenceService)
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

        if (session.UserId != request.UserId)
        {
            logger.LogWarning("EndSession failed: Lecturer {LecturerId} is not assigned to session {SessionId}.",
                request.UserId, request.SessionId);
            throw new BusinessRuleException("LECTURER_NOT_ASSIGNED", "Giảng viên không được phân công cho phiên này.");
        }

        if (!Equals(session.Status, SessionStatus.Active))
        {
            logger.LogWarning(
                "EndSession failed: Session {SessionId} is not in Active status. Current status: {Status}.",
                session.Id, session.Status);
            throw new BusinessRuleException("SESSION_NOT_ACTIVE", "Phiên điểm danh chưa ở trạng thái hoạt động.");
        }

        // 1. Lấy tất cả rounds và phân loại
        var allRounds = await roundRepository.GetRoundsBySessionIdAsync(session.Id, cancellationToken);
        var completedRounds = allRounds.Where(r => Equals(r.Status, RoundStatus.Completed)).ToList();
        var activeRound = allRounds.FirstOrDefault(r => Equals(r.Status, RoundStatus.Active));
        var pendingRounds = allRounds.Where(r => Equals(r.Status, RoundStatus.Pending)).ToList();

        // 2. Tính toán attendance cho round đang active (nếu có)
        if (activeRound is not null)
        {
            logger.LogInformation("Calculating attendance for active round {RoundId}", activeRound.Id);

            var calculationResult = await attendanceCalculationService.CalculateAttendanceForRound(
                session.Id,
                activeRound.Id,
                cancellationToken);

            // THAY ĐỔI: sử dụng service chung
            await attendancePersistenceService.PersistAttendanceResult(activeRound, calculationResult.AttendedDeviceIds,
                cancellationToken);

            // Đánh dấu round active thành completed
            activeRound.CompleteRound();
            await roundRepository.UpdateAsync(activeRound, cancellationToken);

            // Thêm vào danh sách completed để tính tổng kết
            completedRounds.Add(activeRound);

            logger.LogInformation("Active round {RoundId} marked as completed", activeRound.Id);
        }

        // 3. Đánh dấu các round pending thành Finalized
        foreach (var pendingRound in pendingRounds)
        {
            pendingRound.UpdateStatus(RoundStatus.Finalized);
            await roundRepository.UpdateAsync(pendingRound, cancellationToken);
            logger.LogInformation("Pending round {RoundId} marked as Finalized", pendingRound.Id);
        }

        // 4. Lưu các thay đổi round
        await roundRepository.SaveChangesAsync(cancellationToken);

        // 5. Kết thúc Session trong DB
        session.CompleteSession();
        await sessionRepository.UpdateAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Session {SessionId} status updated to Completed.", session.Id);

        // 6. Xóa các cờ trạng thái trong Redis
        var activeScheduleKey = $"active_schedule:{session.ScheduleId}";
        await redisService.RemoveAsync($"session:{session.Id}");
        await redisService.RemoveAsync(activeScheduleKey);
        logger.LogInformation("Redis keys for Session {SessionId} and Schedule {ScheduleId} deleted.",
            session.Id, session.ScheduleId);

        // 7. Gửi event cho xử lý cuối cùng (tính % attendance dựa trên số round thực tế)
        var message = new SessionFinalAttendanceToProcess
        {
            SessionId = session.Id,
            ActualRoundsCount = completedRounds.Count
        };
        await publishEndpoint.Publish(message, cancellationToken);
        logger.LogInformation(
            "SessionFinalAttendanceToProcess message published for Session {SessionId} with {ActualRounds} actual rounds.",
            session.Id, completedRounds.Count);

        // 8. Trả về Response
        return new EndSessionResponse
        {
            SessionId = session.Id,
            Status = session.Status.ToString(),
            EndTime = session.EndTime,
            UpdatedAt = session.UpdatedAt,
            ActualRoundsCompleted = completedRounds.Count,
            RoundsFinalized = pendingRounds.Count
        };
    }
}
