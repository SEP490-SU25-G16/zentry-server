using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.DeviceManagement.Application.Features.RegisterDevice;

public class RegisterDeviceCommand : ICommand<RegisterDeviceResponse>
{
    // UserId is populated by the Controller from JWT claims, NOT sent by the client.
    public Guid UserId { get; set; }

    // This is the DeviceName ValueObject's underlying string value.
    // It's the user-provided name for the device, e.g., "My iPhone".
    public string DeviceName { get; set; } = string.Empty;

    // As per our discussion, DeviceModel, OS, AppVersion are removed from the Command.
    // If needed for logging/auditing, they'd be passed separately or extracted at Presentation.
}
