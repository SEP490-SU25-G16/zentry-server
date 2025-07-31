using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;
// <-- Thêm MediatR
// Thêm nếu dùng ErrorMessages
// <-- Thêm để truy cập GetDeviceByMacIntegrationQuery/Response
// Để truy cập ScannedDeviceContract (nếu không có)

// Thêm nếu dùng NotFoundException

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class SubmitScanDataConsumer(
    ILogger<SubmitScanDataConsumer> logger,
    IScanLogRepository scanLogRepository,
    IRoundRepository roundRepository,
    IMediator mediator) // <-- Thêm IMediator
    : IConsumer<SubmitScanDataMessage>
{
    public async Task Consume(ConsumeContext<SubmitScanDataMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received scan data for SessionId: {SessionId}, Submitter MAC: {SubmitterMac}",
            message.SessionId, message.SubmitterDeviceMacAddress);

        try
        {
            logger.LogInformation(
                "Processing Bluetooth scan data for Session {SessionId}, Submitter MAC {SubmitterMac}",
                message.SessionId, message.SubmitterDeviceMacAddress);

            // --- BƯỚC A: Lấy DeviceId và UserId của thiết bị gửi request (Submitter) từ MAC Address ---
            Guid submitterDeviceId;
            Guid submitterUserId;
            try
            {
                var getSubmitterDeviceQuery = new GetDeviceByMacIntegrationQuery(message.SubmitterDeviceMacAddress);
                var submitterDeviceResponse =
                    await mediator.Send(getSubmitterDeviceQuery, consumeContext.CancellationToken);

                submitterDeviceId = submitterDeviceResponse.DeviceId;
                submitterUserId = submitterDeviceResponse.UserId;

                logger.LogInformation("Submitter Device (MAC: {Mac}) mapped to DeviceId: {DeviceId}, UserId: {UserId}",
                    message.SubmitterDeviceMacAddress, submitterDeviceId, submitterUserId);
            }
            catch (NotFoundException ex)
            {
                logger.LogError(ex,
                    "SubmitScanData failed in Consumer: Submitter device not found or inactive for MAC: {MacAddress}. Skipping.",
                    message.SubmitterDeviceMacAddress);
                // Quyết định: Khi lỗi ở Consumer, không throw để message không bị retry vô hạn nếu đó là lỗi data.
                // Log lỗi và kết thúc xử lý cho message này.
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting submitter device info for MAC: {MacAddress} in Consumer.",
                    message.SubmitterDeviceMacAddress);
                // Nếu đây là lỗi tạm thời (ví dụ: DB down), bạn có thể cân nhắc throw lại để message được retry.
                // Tuy nhiên, với NotFoundException, thường không retry.
                throw; // Throw lại để MassTransit xử lý retry/move to error queue nếu đây là lỗi hệ thống.
            }

            // --- BƯỚC B: Ánh xạ MAC Address của các thiết bị được quét thành DeviceId ---
            var scannedMacAddresses = message.ScannedDevices.Select(sd => sd.MacAddress).ToList();
            var finalScannedDevicesForLog = new List<ScannedDevice>(); // Hoặc dùng [] trong C# 9+

            if (scannedMacAddresses.Any())
            {
                var getScannedDevicesQuery = new GetDevicesByMacListIntegrationQuery(scannedMacAddresses);
                var scannedDevicesResponse =
                    await mediator.Send(getScannedDevicesQuery, consumeContext.CancellationToken);

                var macToDeviceUserMap = scannedDevicesResponse.DeviceMappings.ToDictionary(
                    m => m.MacAddress,
                    m => (m.DeviceId, m.UserId),
                    StringComparer.OrdinalIgnoreCase
                );

                foreach (var scannedDeviceFromMsg in message.ScannedDevices)
                    if (macToDeviceUserMap.TryGetValue(scannedDeviceFromMsg.MacAddress, out var mappedInfo))
                        // SỬA ĐỔI DÒNG NÀY: Thêm 'new' trước ScannedDevice
                        finalScannedDevicesForLog.Add(
                            new ScannedDevice(mappedInfo.DeviceId.ToString(), scannedDeviceFromMsg.Rssi));
                    else
                        logger.LogWarning(
                            "Scanned device with MAC {ScannedMac} not found or inactive during consumer processing, skipping.",
                            scannedDeviceFromMsg.MacAddress);
            }

            // Nếu không có thiết bị hợp lệ nào được quét (ví dụ: tất cả đều không đăng ký)
            if (!finalScannedDevicesForLog.Any())
            {
                logger.LogWarning(
                    "No valid (registered and active) scanned devices found in the message for Session {SessionId}. Scan log will not be created for this message.",
                    message.SessionId);
                return; // Kết thúc xử lý nếu không có thiết bị nào hợp lệ để log
            }

            // 1. Save scan log data
            var record = ScanLog.Create(
                Guid.NewGuid(),
                submitterDeviceId,
                submitterUserId,
                message.SessionId,
                message.RoundId,
                message.Timestamp,
                finalScannedDevicesForLog
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
                "MassTransit Consumer: Fatal error processing scan data for SessionId {SessionId}. Message will be retried or moved to error queue.",
                message.SessionId);
            throw; // Rất quan trọng: throw lại để MassTransit biết và xử lý retry/DLQ
        }
    }
}