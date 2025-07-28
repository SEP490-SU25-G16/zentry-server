using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public class SubmitScanDataCommandHandler(
    IRedisService redisService,
    IRoundRepository roundRepository,
    ILogger<SubmitScanDataCommandHandler> logger,
    IBus bus)
    : ICommandHandler<SubmitScanDataCommand, SubmitScanDataResponse>
{
    public async Task<SubmitScanDataResponse> Handle(SubmitScanDataCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received SubmitScanDataCommand for Session {SessionId}, Device {DeviceId}, SubmitterUser {SubmitterUserId}",
            request.SessionId, request.DeviceId, request.SubmitterUserId);

        var sessionKey = $"session:{request.SessionId}";
        var sessionExists = await redisService.KeyExistsAsync(sessionKey);

        if (!sessionExists)
        {
            logger.LogWarning("SubmitScanData failed: Session {SessionId} not found or not active.",
                request.SessionId);
            throw new BusinessRuleException("SESSION_NOT_ACTIVE",
                "Phiên điểm danh không còn hoạt động hoặc không tồn tại.");
        }

        Guid currentRoundId;
        try
        {
            var allRoundsInSession =
                await roundRepository.GetRoundsBySessionIdAsync(request.SessionId, cancellationToken);

            var currentRound = allRoundsInSession
                .Where(r => request.Timestamp >= r.StartTime &&
                            (r.EndTime == null || request.Timestamp <= r.EndTime) &&
                            !Equals(r.Status, RoundStatus.Completed) &&
                            !Equals(r.Status, RoundStatus.Cancelled) &&
                            !Equals(r.Status, RoundStatus.Finalized))
                .OrderByDescending(r => r.StartTime)
                .FirstOrDefault();

            if (currentRound is null)
            {
                logger.LogWarning(
                    "No active or pending round found for Session {SessionId} at timestamp {Timestamp}. Scan data will be logged without a specific round.",
                    request.SessionId, request.Timestamp);

                throw new ApplicationException("An error occurred while determining the round   .");
            }

            currentRoundId = currentRound.Id;
            logger.LogInformation(
                "Scan data for Session {SessionId} assigned to Round {RoundId} (RoundNumber: {RoundNumber}).",
                request.SessionId, currentRound.Id, currentRound.RoundNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error determining current round for Session {SessionId} at timestamp {Timestamp}.",
                request.SessionId, request.Timestamp);
            throw new ApplicationException("An error occurred while determining the active round.", ex);
        }

        try
        {
            var message = new SubmitScanDataMessage(
                request.DeviceId,
                request.SubmitterUserId,
                request.SessionId,
                currentRoundId,
                request.ScannedDevices.Select(sd => new ScannedDeviceContract(sd.DeviceId, sd.Rssi)).ToList(),
                request.Timestamp
            );

            await bus.Publish(message, cancellationToken);
            logger.LogInformation(
                "Scan data message for Session {SessionId}, User {UserId} published and ScanLog saved with RoundId {RoundId}.",
                request.SessionId, request.SubmitterUserId, currentRoundId);

            return new SubmitScanDataResponse(true, "Dữ liệu quét đã được tiếp nhận và đưa vào hàng đợi xử lý.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish scan data message via MassTransit or save ScanLog for Session {SessionId}, User {UserId}.",
                request.SessionId, request.SubmitterUserId);
            throw new ApplicationException("An error occurred while queuing or saving scan data.", ex);
        }
    }
}
