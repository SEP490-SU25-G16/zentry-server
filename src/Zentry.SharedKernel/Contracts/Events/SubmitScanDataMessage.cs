namespace Zentry.SharedKernel.Contracts.Events;

public record SubmitScanDataMessage(
    string SubmitterDeviceAndroidId,
    Guid SessionId,
    Guid RoundId,
    List<ScannedDeviceContractForMessage> ScannedDevices,
    DateTime Timestamp
);

public record ScannedDeviceContractForMessage(
    string AndroidId,
    int Rssi
);