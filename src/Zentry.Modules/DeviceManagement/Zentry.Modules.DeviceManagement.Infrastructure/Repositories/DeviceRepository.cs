using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Domain.Entities;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    public Task<Device> GetByIdAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Device>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Device> GetByDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Device device, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Device device, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Device device, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
