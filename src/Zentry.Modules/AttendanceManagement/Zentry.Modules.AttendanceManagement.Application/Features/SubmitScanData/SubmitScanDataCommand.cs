using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public record SubmitScanDataCommand(
    Guid DeviceId,
    Guid SubmitterUserId,
    Guid SessionId,
    List<ScannedDevice> ScannedDevices,
    DateTime Timestamp
) : ICommand<SubmitScanDataResponse>;

public record SubmitScanDataResponse(bool Success, string Message);
