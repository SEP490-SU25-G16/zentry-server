using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.EndSession;

public class EndSessionCommandHandler(
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<EndSessionCommandHandler> logger)
    : ICommandHandler<EndSessionCommand, EndSessionResponse>
{
    public async Task<EndSessionResponse> Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to end session {SessionId} by user {UserId}.",
            request.SessionId, request.LecturerId);

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning("EndSession failed: Session with ID {SessionId} not found.", request.SessionId);
            throw new NotFoundException(nameof(EndSessionCommandHandler), request.SessionId);
        }

        if (session.LecturerId != request.LecturerId)
        {
            logger.LogWarning("EndSession failed: Lecturer {LecturerId} is not assigned to session {SessionId}.",
                request.LecturerId, request.SessionId);
            throw new BusinessRuleException("LECTURER_NOT_ASSIGNED", "Giảng viên không được phân công cho phiên này.");
        }

        if (!Equals(session.Status, SessionStatus.Active))
        {
            logger.LogWarning(
                "EndSession failed: Session {SessionId} is not in Active status. Current status: {Status}.",
                session.Id, session.Status);
            throw new BusinessRuleException(ErrorCodes.SessionNotActive, ErrorMessages.Attendance.SessionNotActive);
        }

        // Get all rounds and categorize them
        var allRounds = await roundRepository.GetRoundsBySessionIdAsync(session.Id, cancellationToken);
        var activeRound = allRounds.FirstOrDefault(r => Equals(r.Status, RoundStatus.Active));
        var pendingRounds = allRounds.Where(r => Equals(r.Status, RoundStatus.Pending)).ToList();

        if (activeRound is not null)
        {
            // Publish message to process active round asynchronously with retry capability
            var message = new ProcessActiveRoundForEndSessionMessage
            {
                SessionId = session.Id,
                ActiveRoundId = activeRound.Id,
                UserId = request.LecturerId,
                PendingRoundIds = pendingRounds.Select(pr => pr.Id).ToList()
            };

            await publishEndpoint.Publish(message, cancellationToken);

            logger.LogInformation(
                "End session processing message published for Session {SessionId} with active round {RoundId}",
                session.Id, activeRound.Id);
        }
        else
        {
            // No active round - handle only pending rounds and complete session directly
            logger.LogInformation("No active round found for session {SessionId}, processing directly", session.Id);

            await ProcessEndSessionWithoutActiveRound(session, pendingRounds, cancellationToken);
        }

        // Return response indicating the operation has been queued/processed
        return new EndSessionResponse
        {
            SessionId = session.Id,
            Status = activeRound is not null ? "Processing" : session.Status.ToString(),
            EndTime = session.EndTime,
            UpdatedAt = session.UpdatedAt,
            ActualRoundsCompleted = allRounds.Count(r => Equals(r.Status, RoundStatus.Completed)),
            RoundsFinalized = pendingRounds.Count
        };
    }

    private async Task ProcessEndSessionWithoutActiveRound(
        Session session,
        List<Round> pendingRounds,
        CancellationToken cancellationToken)
    {
        // Mark pending rounds as Finalized
        foreach (var pendingRound in pendingRounds)
        {
            pendingRound.UpdateStatus(RoundStatus.Finalized);
            await roundRepository.UpdateAsync(pendingRound, cancellationToken);
            logger.LogInformation("Pending round {RoundId} marked as Finalized", pendingRound.Id);
        }

        await roundRepository.SaveChangesAsync(cancellationToken);

        // Complete session
        session.CompleteSession();
        await sessionRepository.UpdateAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Session {SessionId} completed directly (no active round)", session.Id);

        // Publish final attendance processing message
        var allRounds = await roundRepository.GetRoundsBySessionIdAsync(session.Id, cancellationToken);
        var completedRoundsCount = allRounds.Count(r => Equals(r.Status, RoundStatus.Completed));

        var finalMessage = new SessionFinalAttendanceToProcessMessage
        {
            SessionId = session.Id,
            ActualRoundsCount = completedRoundsCount
        };
        await publishEndpoint.Publish(finalMessage, cancellationToken);
        logger.LogInformation(
            "SessionFinalAttendanceToProcess message published for Session {SessionId} with {ActualRounds} actual rounds.",
            session.Id, completedRoundsCount);
    }
}