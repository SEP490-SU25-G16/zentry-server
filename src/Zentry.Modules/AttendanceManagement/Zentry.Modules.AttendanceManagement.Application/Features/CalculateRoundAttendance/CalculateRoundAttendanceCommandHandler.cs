using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CalculateRoundAttendance;

public class CalculateRoundAttendanceCommandHandler(
    ILogger<CalculateRoundAttendanceCommandHandler> logger,
    IRedisService redisService,
    IRoundRepository roundRepository,
    ISessionRepository sessionRepository,
    IPublishEndpoint publishEndpoint,
    IAttendanceCalculationService attendanceCalculationService,
    IAttendancePersistenceService attendancePersistenceService) // THAY ĐỔI: sử dụng service riêng
    : ICommandHandler<CalculateRoundAttendanceCommand, CalculateRoundAttendanceResponse>
{
    public async Task<CalculateRoundAttendanceResponse> Handle(
        CalculateRoundAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting attendance calculation for Session {SessionId}, Round {RoundId}",
            request.SessionId, request.RoundId);

        try
        {
            // Validate round exists
            var round = await roundRepository.GetByIdAsync(request.RoundId, cancellationToken);
            if (round is null)
            {
                logger.LogWarning("Round {RoundId} not found", request.RoundId);
                return new CalculateRoundAttendanceResponse(false, "Round not found");
            }

            // Calculate attendance using service
            var calculationResult = await attendanceCalculationService.CalculateAttendanceForRound(
                request.SessionId,
                request.RoundId,
                cancellationToken);

            // THAY ĐỔI: sử dụng service riêng thay vì method private
            await attendancePersistenceService.PersistAttendanceResult(round, calculationResult.AttendedDeviceIds,
                cancellationToken);

            await roundRepository.UpdateRoundStatusAsync(request.RoundId, RoundStatus.Completed, cancellationToken);

            // THAY ĐỔI: sử dụng lecturerId từ kết quả calculation (đã được lấy từ session)
            await CheckAndTriggerFinalAttendanceProcessing(round, calculationResult.LecturerId, cancellationToken);

            logger.LogInformation(
                "Successfully calculated attendance for Round {RoundId}: {Count} devices attended",
                request.RoundId, calculationResult.AttendedDeviceIds.Count);

            return new CalculateRoundAttendanceResponse(
                true,
                "Attendance calculated successfully",
                calculationResult.AttendedDeviceIds.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error calculating attendance for Session {SessionId}, Round {RoundId}",
                request.SessionId, request.RoundId);

            return new CalculateRoundAttendanceResponse(
                false,
                "An error occurred while calculating attendance");
        }
    }

    // XÓA: method PersistAttendanceResult (đã chuyển thành service)

    private async Task CheckAndTriggerFinalAttendanceProcessing(
        Round currentRound,
        string lecturerId, // THAY ĐỔI: giờ đây luôn có giá trị từ session
        CancellationToken cancellationToken)
    {
        var sessionId = currentRound.SessionId;
        var currentRoundNumber = currentRound.RoundNumber;

        var totalRounds = await roundRepository.CountRoundsBySessionIdAsync(sessionId, cancellationToken);

        if (currentRoundNumber == totalRounds)
        {
            logger.LogInformation(
                "Round {RoundNumber} is the final round for Session {SessionId}. Publishing final attendance processing message.",
                currentRoundNumber, sessionId);

            var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
            if (session is null)
            {
                throw new NotFoundException(nameof(CalculateRoundAttendanceCommandHandler), sessionId);
            }

            // THAY ĐỔI: so sánh với UserId trong session
            if (!Equals(session.UserId.ToString(), lecturerId))
            {
                logger.LogWarning("EndSession failed: Lecturer {LecturerId} is not assigned to session {SessionId}.",
                    lecturerId, sessionId);
                throw new BusinessRuleException("LECTURER_NOT_ASSIGNED",
                    "Giảng viên không được phân công cho phiên này.");
            }

            if (!Equals(session.Status, SessionStatus.Active))
            {
                logger.LogWarning(
                    "EndSession failed: Session {SessionId} is not in Active status. Current status: {Status}.",
                    session.Id, session.Status);
                throw new BusinessRuleException("SESSION_NOT_ACTIVE", "Phiên điểm danh chưa ở trạng thái hoạt động.");
            }

            session.CompleteSession();
            await sessionRepository.UpdateAsync(session, cancellationToken);
            await sessionRepository.SaveChangesAsync(cancellationToken);

            var message = new SessionFinalAttendanceToProcess
            {
                SessionId = sessionId,
                ActualRoundsCount = totalRounds
            };
            await publishEndpoint.Publish(message, cancellationToken);

            logger.LogInformation("SessionFinalAttendanceToProcess message published for Session {SessionId}.",
                sessionId);
        }
        else
        {
            logger.LogInformation(
                "Round {RoundNumber} is not the final round ({TotalRounds} total rounds) for Session {SessionId}. No final processing triggered.",
                currentRoundNumber, sessionId, totalRounds);
        }
    }
}
