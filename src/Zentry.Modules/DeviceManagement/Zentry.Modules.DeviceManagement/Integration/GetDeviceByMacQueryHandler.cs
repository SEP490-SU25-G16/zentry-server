using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.DeviceManagement.Integration;

public class GetDeviceByMacQueryHandler(IDeviceRepository repository)
    : IQueryHandler<GetDeviceByMacIntegrationQuery, GetDeviceByMacIntegrationResponse>
{
    public async Task<GetDeviceByMacIntegrationResponse> Handle(GetDeviceByMacIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        var deviceInfo = await repository.GetDeviceAndUserIdByMacAddressAsync(request.MacAddress, cancellationToken);

        if (deviceInfo.HasValue)
        {
            var (deviceId, userId) = deviceInfo.Value;
            return new GetDeviceByMacIntegrationResponse(deviceId, userId, request.MacAddress);
        }

        throw new NotFoundException(nameof(GetDeviceByMacQueryHandler),
            $"Active Device not found for MAC address: {request.MacAddress}");
    }
}