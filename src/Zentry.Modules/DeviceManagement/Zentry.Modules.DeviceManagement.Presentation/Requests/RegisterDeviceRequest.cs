namespace Zentry.Modules.DeviceManagement.Presentation.Requests;

public record RegisterDeviceRequest(Guid AccountId, string DeviceName);