using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Device;

public record GetDeviceByMacIntegrationQuery(string MacAddress)
    : IQuery<GetDeviceByMacIntegrationResponse>;

public record GetDeviceByMacIntegrationResponse(
    Guid DeviceId,
    Guid UserId,
    string MacAddress
);
