using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CalculateRoundAttendance;

public class CalculateRoundAttendanceCommandHandler(
    ILogger<CalculateRoundAttendanceCommandHandler> logger,
    IRoundRepository roundRepository,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CalculateRoundAttendanceCommand, CalculateRoundAttendanceResponse>
{
    public async Task<CalculateRoundAttendanceResponse> Handle(
        CalculateRoundAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Queuing attendance calculation for Session {SessionId}, Round {RoundId}",
            request.SessionId, request.RoundId);

        try
        {
            var round = await roundRepository.GetByIdAsync(request.RoundId, cancellationToken);
            if (round is null)
            {
                logger.LogWarning("Round {RoundId} not found", request.RoundId);
                return new CalculateRoundAttendanceResponse(false, "Round not found");
            }

            // Check if this is the final round
            var totalRounds = await roundRepository.CountRoundsBySessionIdAsync(request.SessionId, cancellationToken);
            var isFinalRound = round.RoundNumber == totalRounds;

            // Publish message for async processing with retry capability
            var message = new CalculateRoundAttendanceMessage
            {
                SessionId = request.SessionId,
                RoundId = request.RoundId,
                IsFinalRound = isFinalRound,
                TotalRounds = totalRounds
            };

            await publishEndpoint.Publish(message, cancellationToken);

            logger.LogInformation(
                "Attendance calculation message published for Session {SessionId}, Round {RoundId}",
                request.SessionId, request.RoundId);

            return new CalculateRoundAttendanceResponse(
                true,
                "Attendance calculation queued successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error queuing attendance calculation for Session {SessionId}, Round {RoundId}",
                request.SessionId, request.RoundId);

            return new CalculateRoundAttendanceResponse(
                false,
                "An error occurred while queuing attendance calculation");
        }
    }
}