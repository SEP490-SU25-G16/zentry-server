using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Device;

public record GetDeviceByAndroidIdIntegrationQuery(string AndroidId)
    : IQuery<GetDeviceByAndroidIdIntegrationResponse>;

public record GetDeviceByAndroidIdIntegrationResponse(
    Guid DeviceId,
    Guid UserId,
    string AndroidId
);
