namespace Zentry.SharedKernel.Contracts.Events;

public record SubmitScanDataMessage(
    string SubmitterDeviceMacAddress,
    Guid SessionId,
    Guid RoundId,
    List<ScannedDeviceContractForMessage> ScannedDevices,
    DateTime Timestamp
);

public record ScannedDeviceContractForMessage(
    string MacAddress,
    int Rssi
);
