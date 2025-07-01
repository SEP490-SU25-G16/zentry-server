namespace Zentry.Modules.DeviceManagement.Application.Features.RegisterDevice;

public class RegisterDeviceResponse
{
    public Guid DeviceId { get; set; }
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } // The string value of the DeviceToken ValueObject
    public DateTime CreatedAt { get; set; }
}
