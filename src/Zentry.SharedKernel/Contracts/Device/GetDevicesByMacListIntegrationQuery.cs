using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Device;

public record GetDevicesByMacListIntegrationQuery(List<string> MacAddresses)
    : IQuery<GetDevicesByMacListIntegrationResponse>;

public record GetDevicesByMacListIntegrationResponse(
    List<DeviceMacMapping> DeviceMappings
);

public record DeviceMacMapping(
    Guid DeviceId,
    Guid UserId,
    string MacAddress
);