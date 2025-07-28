using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public record SubmitScanDataCommand(
    string SubmitterDeviceMacAddress,
    Guid SessionId,
    List<ScannedDeviceFromRequest> ScannedDevices,
    DateTime Timestamp
) : ICommand<SubmitScanDataResponse>;

public record SubmitScanDataResponse(bool Success, string Message);
