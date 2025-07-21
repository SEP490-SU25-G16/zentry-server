using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Contracts.Messages;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public class SubmitScanDataCommandHandler(
    IRedisService redisService,
    IBus bus,
    IScanLogRepository scanLogRepository,
    ILogger<SubmitScanDataCommandHandler> logger)
    : ICommandHandler<SubmitScanDataCommand, SubmitScanDataResponse>
{
    // MassTransit's bus for publishing messages

    public async Task<SubmitScanDataResponse> Handle(SubmitScanDataCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received SubmitScanDataCommand for Session {SessionId}, Device {DeviceId}, SubmitterUser {SubmitterUserId}",
            request.SessionId, request.DeviceId, request.SubmitterUserId);

        // Simulate session check with Redis
        // Replace with your actual Redis key structure and existence check
        var sessionKey = $"session:{request.SessionId}";
        var sessionExists = await redisService.KeyExistsAsync(sessionKey);

        if (!sessionExists)
        {
            logger.LogWarning("SubmitScanData failed: Session {SessionId} not found or not active.",
                request.SessionId);
            throw new BusinessRuleException("SESSION_NOT_ACTIVE",
                "Phiên điểm danh không còn hoạt động hoặc không tồn tại.");
        }

        // Tạo MassTransit message
        var message = new ProcessScanDataMessage(
            request.DeviceId,
            request.SubmitterUserId,
            request.SessionId,
            request.ScannedDevices.Select(sd => new ScannedDeviceContract(sd.DeviceId, sd.Rssi)).ToList(),
            request.Timestamp
        );

        try
        {
            // Publish the message. MassTransit will handle routing to RabbitMQ.
            // Using Publish is suitable here as multiple consumers could potentially listen to this message type.
            await bus.Publish(message, cancellationToken);

            // Ghi ScanLog. ScanLog entity cần được cập nhật
            var record = ScanLog.Create(
                Guid.NewGuid(),
                request.DeviceId,
                request.SubmitterUserId,
                request.SessionId,
                request.Timestamp,
                request.ScannedDevices.Select(sd => new ScannedDevice(sd.DeviceId, sd.Rssi)).ToList()
            );

            await scanLogRepository.AddScanDataAsync(record);
            logger.LogInformation(
                "Scan data message for Session {SessionId}, User {UserId} published via MassTransit.",
                request.SessionId, request.SubmitterUserId);

            return new SubmitScanDataResponse(true, "Dữ liệu quét đã được tiếp nhận và đưa vào hàng đợi xử lý.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish scan data message via MassTransit for Session {SessionId}, User {UserId}.",
                request.SessionId, request.SubmitterUserId);
            // Re-throw or wrap in a custom exception if you want to distinguish messaging failures
            throw new ApplicationException("An error occurred while queueing scan data for processing.", ex);
        }
    }
}
