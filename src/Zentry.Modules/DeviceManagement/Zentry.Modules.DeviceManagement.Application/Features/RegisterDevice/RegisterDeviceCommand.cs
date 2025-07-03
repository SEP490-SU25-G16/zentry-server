using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.DeviceManagement.Application.Features.RegisterDevice;

public class RegisterDeviceCommand : ICommand<RegisterDeviceResponse>
{
    // UserId is populated by the Controller from JWT claims, NOT sent by the client.
    public Guid UserId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string? Platform { get; set; }
    public string? OsVersion { get; set; }
    public string? Model { get; set; }
    public string? Manufacturer { get; set; }
    public string? AppVersion { get; set; }
    public string? PushNotificationToken { get; set; }
}