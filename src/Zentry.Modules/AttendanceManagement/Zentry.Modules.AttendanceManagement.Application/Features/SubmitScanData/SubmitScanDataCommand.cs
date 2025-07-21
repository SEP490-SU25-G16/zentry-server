using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public record SubmitScanDataCommand(
    Guid DeviceId,
    Guid UserId,
    Guid SessionId,
    string RequestId,
    string RssiData,
    List<string> NearbyDevices,
    DateTime Timestamp
) : ICommand<SubmitScanDataResponse>;

public record SubmitScanDataResponse(bool Success, string Message);