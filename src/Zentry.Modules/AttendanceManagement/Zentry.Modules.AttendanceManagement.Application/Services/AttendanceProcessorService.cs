// Updated AttendanceProcessorService.cs - Simplified version

using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Enums.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class AttendanceProcessorService(
    ILogger<AttendanceProcessorService> logger,
    IScanLogRepository scanLogRepository,
    IRoundRepository roundRepository)
    : IAttendanceProcessorService
{
    public async Task ProcessBluetoothScanData(ProcessScanDataMessage message, CancellationToken cancellationToken)
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

        // 2. Update round status if needed
        var currentRound = await roundRepository.GetByIdAsync(message.RoundId, cancellationToken);
        if (currentRound is null && Equals(currentRound?.Status, RoundStatus.Pending))
        {
            currentRound.UpdateStatus(RoundStatus.Active);
            await roundRepository.UpdateAsync(currentRound, cancellationToken);
            logger.LogInformation("Updated Round {RoundId} status to Active.", currentRound.Id);
        }

        logger.LogInformation("Finished processing scan data for SessionId: {SessionId}.", message.SessionId);
    }
}
