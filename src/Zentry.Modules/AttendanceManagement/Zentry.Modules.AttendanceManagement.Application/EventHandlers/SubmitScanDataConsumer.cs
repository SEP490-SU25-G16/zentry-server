using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class SubmitScanDataConsumer(
    ILogger<SubmitScanDataConsumer> logger,
    IScanLogRepository scanLogRepository,
    IRoundRepository roundRepository)
    : IConsumer<SubmitScanDataMessage>
{
    public async Task Consume(ConsumeContext<SubmitScanDataMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received scan data for SessionId: {SessionId}, Device: {DeviceId}, Submitter: {SubmitterUserId}",
            message.SessionId, message.DeviceId, message.SubmitterUserId);

        try
        {
            logger.LogInformation(
                "Processing Bluetooth scan data for Session {SessionId}, Device {DeviceId}, User {UserId}",
                message.SessionId, message.DeviceId, message.SubmitterUserId);

            // 1. Save scan log data
            var record = ScanLog.Create(
                Guid.NewGuid(),
                message.DeviceId,
                message.SubmitterUserId,
                message.SessionId,
                message.RoundId,
                message.Timestamp,
                message.ScannedDevices.Select(sd => new ScannedDevice(sd.DeviceId, sd.Rssi)).ToList()
            );

            await scanLogRepository.AddScanDataAsync(record);
            logger.LogInformation("Scan log saved for Session {SessionId}, Round {RoundId}",
                message.SessionId, message.RoundId);
            await roundRepository.UpdateRoundStatusAsync(message.RoundId, RoundStatus.Active);
            logger.LogInformation("Finished processing scan data for SessionId: {SessionId}.", message.SessionId);
            logger.LogInformation("MassTransit Consumer: Successfully processed scan data for SessionId: {SessionId}.",
                message.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error processing scan data for SessionId {SessionId}. Message will be retried or moved to error queue.",
                message.SessionId);
            throw;
        }
    }
}
