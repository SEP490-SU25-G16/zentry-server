using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Device;

namespace Zentry.Modules.DeviceManagement.Integration;

public class GetDevicesByMacListQueryHandler(IDeviceRepository repository)
    : IQueryHandler<GetDevicesByMacListIntegrationQuery, GetDevicesByMacListIntegrationResponse>
{
    public async Task<GetDevicesByMacListIntegrationResponse> Handle(GetDevicesByMacListIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        if (request.MacAddresses == null || !request.MacAddresses.Any())
            return new GetDevicesByMacListIntegrationResponse(new List<DeviceMacMapping>());

        var deviceMappings =
            await repository.GetDeviceAndUserIdsByMacAddressesAsync(request.MacAddresses, cancellationToken);

        var result = deviceMappings.Select(mapping => new DeviceMacMapping(
            mapping.Value.DeviceId,
            mapping.Value.UserId,
            mapping.Key // MAC address
        )).ToList();

        return new GetDevicesByMacListIntegrationResponse(result);
    }
}