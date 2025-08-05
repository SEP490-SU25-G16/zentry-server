using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.DeviceManagement.Integration;

public class GetDeviceByAndroidIdQueryHandler(IDeviceRepository repository)
    : IQueryHandler<GetDeviceByAndroidIdIntegrationQuery, GetDeviceByAndroidIdIntegrationResponse>
{
    public async Task<GetDeviceByAndroidIdIntegrationResponse> Handle(GetDeviceByAndroidIdIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        var deviceInfo = await repository.GetDeviceAndUserIdByAndroidIdAsync(request.AndroidId, cancellationToken);

        if (deviceInfo.HasValue)
        {
            var (deviceId, userId) = deviceInfo.Value;
            return new GetDeviceByAndroidIdIntegrationResponse(deviceId, userId, request.AndroidId);
        }

        throw new NotFoundException(nameof(GetDeviceByAndroidIdQueryHandler),
            $"Active Device not found for Android ID: {request.AndroidId}");
    }
}
