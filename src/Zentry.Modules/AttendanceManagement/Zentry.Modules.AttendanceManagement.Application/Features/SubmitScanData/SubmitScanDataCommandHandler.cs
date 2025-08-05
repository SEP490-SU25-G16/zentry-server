using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public class SubmitScanDataCommandHandler(
    IRedisService redisService,
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    ILogger<SubmitScanDataCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<SubmitScanDataCommand, SubmitScanDataResponse>
{
    public async Task<SubmitScanDataResponse> Handle(SubmitScanDataCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received SubmitScanDataCommand for Session {SessionId}, SubmitterDeviceMac {SubmitterMac}",
            request.SessionId, request.SubmitterDeviceMacAddress);

        // Validate session exists and is active
        await ValidateSessionAsync(request.SessionId, request.Timestamp, cancellationToken);

        // Determine current round
        var currentRoundId = await DetermineCurrentRoundAsync(request.SessionId, request.Timestamp, cancellationToken);

        // Publish message - MassTransit sẽ tự động retry theo cấu hình
        var message = new SubmitScanDataMessage(
            request.SubmitterDeviceMacAddress,
            request.SessionId,
            currentRoundId,
            request.ScannedDevices.Select(sd => new ScannedDeviceContractForMessage(sd.MacAddress, sd.Rssi))
                .ToList(),
            request.Timestamp
        );

        await publishEndpoint.Publish(message, cancellationToken);

        logger.LogInformation(
            "Scan data message for Session {SessionId}, Submitter MAC {SubmitterMac} published successfully with RoundId {RoundId}",
            request.SessionId, request.SubmitterDeviceMacAddress, currentRoundId);

        return new SubmitScanDataResponse(true, "Dữ liệu quét đã được tiếp nhận và đưa vào hàng đợi xử lý.");
    }

    private async Task ValidateSessionAsync(Guid sessionId, DateTime timestamp, CancellationToken cancellationToken)
    {
        var sessionKey = $"session:{sessionId}";
        var sessionExists = await redisService.KeyExistsAsync(sessionKey);

        if (!sessionExists)
        {
            var actualEndTime = await sessionRepository.GetActualEndTimeAsync(sessionId, cancellationToken);

            if (actualEndTime.HasValue && timestamp > actualEndTime.Value)
            {
                logger.LogWarning(
                    "SubmitScanData rejected: Data timestamp {Timestamp} is after actual session end time {ActualEndTime} for Session {SessionId}",
                    timestamp, actualEndTime, sessionId);

                throw new SessionEndedException(ErrorMessages.Attendance.SessionEnded);
            }

            logger.LogWarning("SubmitScanData failed: Session {SessionId} not found or not active", sessionId);
            throw new BusinessRuleException(ErrorCodes.SessionNotActive,
                ErrorMessages.Attendance.SessionNotActive);
        }
    }

    private async Task<Guid> DetermineCurrentRoundAsync(Guid sessionId, DateTime timestamp, CancellationToken cancellationToken)
    {
        try
        {
            var allRoundsInSession = await roundRepository.GetRoundsBySessionIdAsync(sessionId, cancellationToken);

            var currentRound = allRoundsInSession
                .Where(r => timestamp >= r.StartTime &&
                            timestamp <= r.EndTime &&
                            !Equals(r.Status, RoundStatus.Completed) &&
                            !Equals(r.Status, RoundStatus.Cancelled) &&
                            !Equals(r.Status, RoundStatus.Finalized))
                .OrderByDescending(r => r.StartTime)
                .FirstOrDefault();

            if (currentRound is null)
            {
                logger.LogWarning(
                    "No active or pending round found for Session {SessionId} at timestamp {Timestamp}",
                    sessionId, timestamp);
                throw new ApplicationException("An error occurred while determining the round.");
            }

            logger.LogInformation(
                "Scan data for Session {SessionId} assigned to Round {RoundId} (RoundNumber: {RoundNumber})",
                sessionId, currentRound.Id, currentRound.RoundNumber);

            return currentRound.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error determining current round for Session {SessionId} at timestamp {Timestamp}",
                sessionId, timestamp);
            throw new ApplicationException("An error occurred while determining the active round.", ex);
        }
    }
}
