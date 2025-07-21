using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.SharedKernel.Contracts.Messages;

public record ProcessScanDataMessage(
    Guid DeviceId,
    Guid SubmitterUserId,
    Guid SessionId,
    List<ScannedDeviceContract> ScannedDevices,
    DateTime Timestamp
);
