using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.DeviceManagement.Features.GetTotalDevices;

public class GetTotalDevicesQueryHandler(
    IDeviceRepository deviceRepository
) : IQueryHandler<GetTotalDevicesQuery, GetTotalDevicesResponse>
{
    public async Task<GetTotalDevicesResponse> Handle(GetTotalDevicesQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await deviceRepository.CountAllAsync(cancellationToken);
        return new GetTotalDevicesResponse(totalCount);
    }
}
