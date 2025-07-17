namespace Zentry.Modules.AttendanceManagement.Presentation.Requests;

public record SubmitScanDataRequestDto(
    Guid DeviceId,
    Guid UserId,
    Guid SessionId,
    string RequestId,
    string RssiData,
    List<string> NearbyDevices,
    DateTime Timestamp
);
