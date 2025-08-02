using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;
// Vẫn cần MediatR nếu các command/query khác trong module này sử dụng nó.
// Nếu SubmitScanDataCommandHandler là handler duy nhất cần MediatR, có thể bỏ.

// using Zentry.SharedKernel.Contracts.Device; // KHÔNG CẦN THIẾT NỮA

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public class SubmitScanDataCommandHandler(
    IRedisService redisService,
    IRoundRepository roundRepository,
    ILogger<SubmitScanDataCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<SubmitScanDataCommand, SubmitScanDataResponse>
{
    public async Task<SubmitScanDataResponse> Handle(SubmitScanDataCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received SubmitScanDataCommand for Session {SessionId}, SubmitterDeviceMac {SubmitterMac}. Publishing message to consumer.",
            request.SessionId, request.SubmitterDeviceMacAddress);

        // --- BỎ BƯỚC 1: Lấy DeviceId và UserId của thiết bị gửi request (Submitter) từ MAC Address ---
        // Logic này sẽ được chuyển xuống consumer

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
                throw new ApplicationException("An error occurred while determining the round.");
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

        // --- BỎ BƯỚC 3: Ánh xạ MAC Address của các thiết bị được quét thành DeviceId ---
        // Logic này cũng sẽ được chuyển xuống consumer

        // --- BƯỚC 4: Publish message với MAC Addresses thô ---
        try
        {
            var message = new SubmitScanDataMessage(
                request.SubmitterDeviceMacAddress, // Truyền MAC Address của thiết bị gửi
                request.SessionId,
                currentRoundId,
                request.ScannedDevices.Select(sd => new ScannedDeviceContractForMessage(sd.MacAddress, sd.Rssi))
                    .ToList(), // Dùng ScannedDeviceContractForMessage
                request.Timestamp
            );

            await publishEndpoint.Publish(message, cancellationToken);
            logger.LogInformation(
                "Scan data message for Session {SessionId}, Submitter MAC {SubmitterMac} published with RoundId {RoundId}.",
                request.SessionId, request.SubmitterDeviceMacAddress, currentRoundId);

            return new SubmitScanDataResponse(true, "Dữ liệu quét đã được tiếp nhận và đưa vào hàng đợi xử lý.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish scan data message via MassTransit for Session {SessionId}, Submitter MAC {SubmitterMac}.",
                request.SessionId, request.SubmitterDeviceMacAddress);
            throw new ApplicationException("An error occurred while queuing scan data.", ex);
        }
    }
}
