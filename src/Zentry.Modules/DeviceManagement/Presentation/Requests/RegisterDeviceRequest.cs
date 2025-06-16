namespace Presentation.Requests;

public record RegisterDeviceRequest(Guid AccountId, string DeviceName);