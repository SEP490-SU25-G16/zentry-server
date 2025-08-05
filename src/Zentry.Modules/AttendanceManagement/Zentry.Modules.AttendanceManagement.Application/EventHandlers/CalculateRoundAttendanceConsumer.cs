using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class CalculateRoundAttendanceConsumer(
    ILogger<CalculateRoundAttendanceConsumer> logger,
    IRoundRepository roundRepository,
    ISessionRepository sessionRepository,
    IPublishEndpoint publishEndpoint,
    IAttendanceCalculationService attendanceCalculationService,
    IAttendancePersistenceService attendancePersistenceService)
    : IConsumer<CalculateRoundAttendanceMessage>
{
    public async Task Consume(ConsumeContext<CalculateRoundAttendanceMessage> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing attendance calculation for Session {SessionId}, Round {RoundId}",
            message.SessionId, message.RoundId);

        try
        {
            var round = await roundRepository.GetByIdAsync(message.RoundId, context.CancellationToken);
            if (round is null)
            {
                logger.LogWarning("Round {RoundId} not found", message.RoundId);
                throw new NotFoundException(nameof(CalculateRoundAttendanceConsumer), message.RoundId);
            }

            // Calculate attendance using service - this will throw if scan data doesn't exist
            var calculationResult = await attendanceCalculationService.CalculateAttendanceForRound(
                message.SessionId,
                message.RoundId,
                context.CancellationToken);

            // Persist results
            await attendancePersistenceService.PersistAttendanceResult(
                round,
                calculationResult.AttendedDeviceIds,
                context.CancellationToken);

            // Update round status
            await roundRepository.UpdateRoundStatusAsync(
                message.RoundId,
                RoundStatus.Completed,
                context.CancellationToken);

            logger.LogInformation(
                "Successfully calculated attendance for Round {RoundId}: {Count} devices attended",
                message.RoundId, calculationResult.AttendedDeviceIds.Count);

            // Check if this is the final round and trigger final processing
            if (message.IsFinalRound)
                await TriggerFinalAttendanceProcessing(
                    message.SessionId,
                    message.TotalRounds,
                    context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error calculating attendance for Session {SessionId}, Round {RoundId}. Will retry.",
                message.SessionId, message.RoundId);

            // Rethrow to trigger retry mechanism
            throw;
        }
    }

    private async Task TriggerFinalAttendanceProcessing(
        Guid sessionId,
        int totalRounds,
        CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null) throw new NotFoundException(nameof(CalculateRoundAttendanceConsumer), sessionId);


        //
        // // Validate lecturer assignment
        // if (!Equals(session.UserId.ToString(), lecturerId))
        // {
        //     logger.LogWarning("Final processing failed: Lecturer {LecturerId} is not assigned to session {SessionId}.",
        //         lecturerId, sessionId);
        //     throw new BusinessRuleException("LECTURER_NOT_ASSIGNED",
        //         "Giảng viên không được phân công cho phiên này.");
        // }

        if (!(Equals(session.Status, SessionStatus.Active) || Equals(session.Status, SessionStatus.Completed)))
        {
            logger.LogWarning(
                "Final processing failed: Session {SessionId} is not in Active status. Current status: {Status}.",
                session.Id, session.Status);
            throw new BusinessRuleException(ErrorCodes.SessionNotActiveOrComplete,
                ErrorMessages.Attendance.SessionNotActiveOrComplete);
        }

        // Complete session
        session.CompleteSession();
        await sessionRepository.UpdateAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        // Publish final attendance processing message
        var finalMessage = new SessionFinalAttendanceToProcess
        {
            SessionId = sessionId,
            ActualRoundsCount = totalRounds
        };
        await publishEndpoint.Publish(finalMessage, cancellationToken);

        logger.LogInformation("SessionFinalAttendanceToProcess message published for Session {SessionId}.",
            sessionId);
    }
}