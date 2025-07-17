using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
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
            "Received SubmitScanDataCommand for Session {SessionId}, User {UserId}, Device {DeviceId}",
            request.SessionId, request.UserId, request.DeviceId);

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

        // Create the MassTransit message
        var message = new ProcessScanDataMessage(
            request.SessionId,
            request.UserId,
            request.DeviceId,
            request.RequestId,
            request.RssiData,
            request.NearbyDevices,
            request.Timestamp
        );

        try
        {
            // Publish the message. MassTransit will handle routing to RabbitMQ.
            // Using Publish is suitable here as multiple consumers could potentially listen to this message type.
            await bus.Publish(message, cancellationToken);

            var record = ScanLog.Create
            (
                request.DeviceId,
                request.UserId,
                request.SessionId,
                request.RequestId,
                request.RssiData,
                request.NearbyDevices,
                request.Timestamp
            );

            await scanLogRepository.AddScanDataAsync(record);
            logger.LogInformation(
                "Scan data message for Session {SessionId}, User {UserId} published via MassTransit.",
                request.SessionId, request.UserId);

            return new SubmitScanDataResponse(true, "Dữ liệu quét đã được tiếp nhận và đưa vào hàng đợi xử lý.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish scan data message via MassTransit for Session {SessionId}, User {UserId}.",
                request.SessionId, request.UserId);
            // Re-throw or wrap in a custom exception if you want to distinguish messaging failures
            throw new ApplicationException("An error occurred while queueing scan data for processing.", ex);
        }
    }
}
