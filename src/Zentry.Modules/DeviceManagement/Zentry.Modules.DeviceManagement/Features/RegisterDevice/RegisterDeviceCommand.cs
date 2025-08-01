using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.DeviceManagement.Features.RegisterDevice;

public class RegisterDeviceCommand : ICommand<RegisterDeviceResponse>
{
    public Guid UserId { get; set; }

    public string DeviceName { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty; // Thêm MAC address - bắt buộc

    public string? Platform { get; set; }
    public string? OsVersion { get; set; }
    public string? Model { get; set; }
    public string? Manufacturer { get; set; }
    public string? AppVersion { get; set; }
    public string? PushNotificationToken { get; set; }
}

public class RegisterDeviceResponse
{
    public Guid DeviceId { get; set; }
    public Guid UserId { get; set; }
    public string? DeviceToken { get; set; } // The string value of the DeviceToken ValueObject
    public string MacAddress { get; set; } = string.Empty; // Trả về MAC address đã được normalize
    public DateTime CreatedAt { get; set; }
}
