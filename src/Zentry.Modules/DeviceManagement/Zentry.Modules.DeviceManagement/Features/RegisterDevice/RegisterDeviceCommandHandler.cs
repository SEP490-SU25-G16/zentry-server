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
    private readonly IMediator _mediator = mediator; // For publishing domain events if needed

    public async Task<RegisterDeviceResponse> Handle(RegisterDeviceCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate UserId existence (from Identity Module)
        var userExists = await userDeviceService.CheckUserExistsAsync(command.UserId, cancellationToken);
        if (!userExists)
            // This scenario should ideally be caught by authentication middleware,
            // but good to have a robust check.
            throw new BusinessLogicException("User not found."); // Or custom NotFoundException

        // 2. Check if user already has an active device
        var existingDevice = await deviceRepository.GetActiveDeviceByUserIdAsync(command.UserId);
        if (existingDevice != null)
            // As per business rule: "Chỉ có 1 thiết bị cho 1 user"
            throw new BusinessLogicException("User already has a primary device registered.");

        // 3. Create DeviceName ValueObject
        DeviceName deviceNameVo;
        try
        {
            deviceNameVo = DeviceName.Create(command.DeviceName);
        }
        catch (ArgumentException ex)
        {
            throw new BusinessLogicException($"Invalid device name: {ex.Message}");
        }

        // 4. Generate DeviceToken ValueObject
        var deviceTokenVo = DeviceToken.Create();

        // 5. Create the new Device entity using the factory method
        //    Truyền tất cả các trường optional từ command vào phương thức Create của entity Device
        var newDevice = Device.Create(
            command.UserId,
            deviceNameVo,
            deviceTokenVo,
            command.Platform,
            command.OsVersion,
            command.Model,
            command.Manufacturer,
            command.AppVersion,
            command.PushNotificationToken
        );

        // 6. Add the new device to the repository
        await deviceRepository.AddAsync(newDevice);

        // 7. Save changes to the database
        await deviceRepository.SaveChangesAsync(cancellationToken);

        // 8. (Optional) Publish Domain Event if the Device entity collected any
        // if (newDevice.DomainEvents.Any())
        // {
        //     foreach (var domainEvent in newDevice.DomainEvents)
        //     {
        //         await _mediator.Publish(domainEvent, cancellationToken);
        //     }
        //     newDevice.ClearDomainEvents();
        // }

        // 9. Return response DTO
        return new RegisterDeviceResponse
        {
            DeviceId = newDevice.Id,
            UserId = newDevice.UserId,
            DeviceToken = newDevice.DeviceToken.Value, // Return the string value of the token
            CreatedAt = newDevice.CreatedAt
            // Nếu muốn, bạn có thể trả về các thông tin optional đã lưu vào DB
            // Platform = newDevice.Platform,
            // OSVersion = newDevice.OSVersion,
            // ...
        };
    }
}
