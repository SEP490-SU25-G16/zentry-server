namespace Zentry.SharedKernel.Contracts.Events;

public record SubmitScanDataMessage(
    string SubmitterDeviceMacAddress, // MAC Address của thiết bị gửi
    Guid SessionId,
    Guid RoundId,
    List<ScannedDeviceContractForMessage> ScannedDevices, // Danh sách các thiết bị được quét (với MAC Address)
    DateTime Timestamp
);

// Tạo một Contract mới để truyền MAC Address trong message
// Đây là dữ liệu thô từ scanner
public record ScannedDeviceContractForMessage(
    string MacAddress, // MAC Address của thiết bị được quét
    int Rssi
);