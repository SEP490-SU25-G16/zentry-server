using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

// THÊM DÒNG NÀY ĐỂ TRUY CẬP RequestType

namespace Zentry.Modules.DeviceManagement.Features.RequestDeviceChange;

public class RequestDeviceChangeCommandHandler(
    IDeviceRepository deviceRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<RequestDeviceChangeCommandHandler> logger
) : ICommandHandler<RequestDeviceChangeCommand, RequestDeviceChangeResponse>
{
    public async Task<RequestDeviceChangeResponse> Handle(RequestDeviceChangeCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("User {UserId} is requesting a device change for MAC address {MacAddress}.",
            command.UserId, command.MacAddress);

        // TODO: Config MaxAllowedDevicesPerUser
        const int maxAllowedDevicesPerUser = 4; // Max N devices (active + inactive + pending)

        var totalUserDevices = await deviceRepository.CountAllByUserIdAsync(command.UserId, cancellationToken);
        if (totalUserDevices >= maxAllowedDevicesPerUser)
        {
            logger.LogWarning(
                "Device change request failed for user {UserId}: Max devices limit reached ({TotalDevices}/{MaxAllowed}).",
                command.UserId, totalUserDevices, maxAllowedDevicesPerUser);
            throw new BusinessRuleException(
                "MAX_DEVICES_REACHED",
                $"Bạn đã đạt giới hạn số lượng thiết bị đăng ký ({totalUserDevices}/{maxAllowedDevicesPerUser}). Vui lòng dùng thiết bị cũ hoặc liên hệ Admin."
            );
        }

        DeviceName deviceNameVo;
        try
        {
            deviceNameVo = DeviceName.Create(command.DeviceName);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "Device change request failed for user {UserId}: Invalid device name '{DeviceName}'. Error: {Message}",
                command.UserId, command.DeviceName, ex.Message);
            throw new BusinessRuleException("INVALID_DEVICE_NAME", $"Tên thiết bị không hợp lệ: {ex.Message}");
        }

        MacAddress macAddressVo;
        try
        {
            macAddressVo = MacAddress.Create(command.MacAddress);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "Device change request failed for user {UserId}: Invalid MAC address '{MacAddress}'. Error: {Message}",
                command.UserId, command.MacAddress, ex.Message);
            throw new BusinessRuleException("INVALID_MAC_ADDRESS", $"Địa chỉ MAC không hợp lệ: {ex.Message}");
        }

        var existingDeviceByMac = await deviceRepository.GetByMacAddressAsync(command.MacAddress, cancellationToken);
        if (existingDeviceByMac is not null)
        {
            if (existingDeviceByMac.UserId == command.UserId)
            {
                if (existingDeviceByMac.Status.Equals(DeviceStatus.Active)) // Sử dụng .Equals cho Enumeration
                {
                    logger.LogWarning(
                        "Device change request failed for user {UserId}: MAC address {MacAddress} is already active for this user.",
                        command.UserId, command.MacAddress);
                    throw new BusinessRuleException("DEVICE_ALREADY_ACTIVE_FOR_USER",
                        "Thiết bị này đã được đăng ký và đang hoạt động cho tài khoản của bạn.");
                }

                logger.LogWarning(
                    "Device change request for user {UserId}: MAC address {MacAddress} already exists but is not active. A new record will be created as per policy.",
                    command.UserId, command.MacAddress);
            }
            else
            {
                logger.LogError(
                    "Device change request for user {UserId}: MAC address {MacAddress} already exists for another user {ExistingUserId}. This indicates a potential data integrity issue.",
                    command.UserId, command.MacAddress, existingDeviceByMac.UserId);
                throw new BusinessRuleException("MAC_ADDRESS_TAKEN",
                    "Địa chỉ MAC này đã được đăng ký bởi người dùng khác.");
            }
        }

        var deviceTokenVo = DeviceToken.Create();

        var newDevice = Device.Create(
            command.UserId,
            deviceNameVo,
            deviceTokenVo,
            macAddressVo,
            command.Platform,
            command.OsVersion,
            command.Model,
            command.Manufacturer,
            command.AppVersion,
            command.PushNotificationToken
        );

        newDevice.Update(
            deviceNameVo,
            DeviceStatus.Pending,
            macAddressVo,
            command.Platform,
            command.OsVersion,
            command.Model,
            command.Manufacturer,
            command.AppVersion,
            command.PushNotificationToken
        );

        await deviceRepository.AddAsync(newDevice, cancellationToken);
        await deviceRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "New device {NewDeviceId} with MAC {MacAddress} created with status Pending for user {UserId}.",
            newDevice.Id, newDevice.MacAddress.Value, newDevice.UserId);

        var requestUpdateDeviceMessage = new RequestUpdateDeviceMessage(
            command.UserId,
            newDevice.Id,
            RequestType.UpdateDevice.ToString(),
            command.Reason
        );

        await publishEndpoint.Publish(requestUpdateDeviceMessage, cancellationToken);
        logger.LogInformation(
            "MassTransit event RequestUpdateDeviceMessage published for user {UserId}, new device {NewDeviceId}.",
            command.UserId, newDevice.Id);

        return new RequestDeviceChangeResponse
        {
            NewDeviceId = newDevice.Id,
            Status = DeviceStatus.Pending.ToString(),
            Message = "Yêu cầu thay đổi thiết bị của bạn đã được gửi và đang chờ phê duyệt."
        };
    }
}