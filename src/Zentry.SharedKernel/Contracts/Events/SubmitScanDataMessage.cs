using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.SharedKernel.Contracts.Events;

public record SubmitScanDataMessage(
    Guid DeviceId,
    Guid SubmitterUserId,
    Guid SessionId,
    Guid RoundId,
    List<ScannedDeviceContract> ScannedDevices,
    DateTime Timestamp
);