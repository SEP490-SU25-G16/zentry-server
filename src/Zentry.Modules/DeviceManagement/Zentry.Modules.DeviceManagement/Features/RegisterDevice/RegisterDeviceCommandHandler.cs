using MediatR;
using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.DeviceManagement.Features.RegisterDevice;

public class RegisterDeviceCommandHandler(
    IDeviceRepository deviceRepository,
    IUserDeviceService userDeviceService,
    IMediator mediator)
    : ICommandHandler<RegisterDeviceCommand, RegisterDeviceResponse>
{
    public async Task<RegisterDeviceResponse> Handle(RegisterDeviceCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate UserId existence (from Identity Module)
        var userExists = await userDeviceService.CheckUserExistsAsync(command.UserId, cancellationToken);
        if (!userExists)
            throw new UserNotFoundException("User not found.");

        // 2. Check if MAC address already exists (MAC addresses should be unique across all devices)
        var existingDeviceByMac = await deviceRepository.GetByMacAddressAsync(command.MacAddress, cancellationToken);
        if (existingDeviceByMac is not null)
            throw new DeviceAlreadyRegisteredException($"Device with MAC address {command.MacAddress} already exists.");

        // 3. Check if user already has an active device
        var existingDevice = await deviceRepository.GetActiveDeviceByUserIdAsync(command.UserId, cancellationToken);
        if (existingDevice != null)
            throw new DeviceAlreadyRegisteredException("User already has a primary device registered.");

        // 4. Create DeviceName ValueObject
        DeviceName deviceNameVo;
        try
        {
            deviceNameVo = DeviceName.Create(command.DeviceName);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid device name: {ex.Message}");
        }

        // 5. Create MacAddress ValueObject with validation
        MacAddress macAddressVo;
        try
        {
            macAddressVo = MacAddress.Create(command.MacAddress);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid MAC address: {ex.Message}");
        }

        // 6. Generate DeviceToken ValueObject
        var deviceTokenVo = DeviceToken.Create();

        // 7. Create the new Device entity using the factory method
        //    Truyền tất cả các trường bao gồm MAC address từ command vào phương thức Create của entity Device
        var newDevice = Device.Create(
            command.UserId,
            deviceNameVo,
            deviceTokenVo,
            macAddressVo, // Thêm MAC address vào
            command.Platform,
            command.OsVersion,
            command.Model,
            command.Manufacturer,
            command.AppVersion,
            command.PushNotificationToken
        );

        // 8. Add the new device to the repository
        await deviceRepository.AddAsync(newDevice);

        // 9. Save changes to the database
        await deviceRepository.SaveChangesAsync(cancellationToken);

        // 10. (Optional) Publish Domain Event if the Device entity collected any
        // if (newDevice.DomainEvents.Any())
        // {
        //     foreach (var domainEvent in newDevice.DomainEvents)
        //     {
        //         await _mediator.Publish(domainEvent, cancellationToken);
        //     }
        //     newDevice.ClearDomainEvents();
        // }

        // 11. Return response DTO với MAC address đã được normalize
        return new RegisterDeviceResponse
        {
            DeviceId = newDevice.Id,
            UserId = newDevice.UserId,
            DeviceToken = newDevice.DeviceToken.Value, // Return the string value of the token
            MacAddress = newDevice.MacAddress.Value, // Return normalized MAC address
            CreatedAt = newDevice.CreatedAt
            // Nếu muốn, bạn có thể trả về các thông tin optional đã lưu vào DB
            // Platform = newDevice.Platform,
            // OSVersion = newDevice.OSVersion,
            // ...
        };
    }
}
