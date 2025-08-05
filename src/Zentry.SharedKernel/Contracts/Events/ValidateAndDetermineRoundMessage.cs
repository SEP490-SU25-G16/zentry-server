using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.SharedKernel.Contracts.Events;

public record ValidateAndDetermineRoundMessage
{
    public string SubmitterDeviceMacAddress { get; init; } = string.Empty;
    public Guid SessionId { get; init; }
    public List<ScannedDeviceContract> ScannedDevices { get; init; } = [];
    public DateTime Timestamp { get; init; }
}
