namespace Zentry.SharedKernel.Contracts.Messages;

public record ProcessScanDataMessage(
    Guid SessionId,
    Guid StudentId,
    Guid DeviceId,
    string RequestId,
    string RssiData,
    List<string> NearbyDevices,
    DateTime Timestamp
);
