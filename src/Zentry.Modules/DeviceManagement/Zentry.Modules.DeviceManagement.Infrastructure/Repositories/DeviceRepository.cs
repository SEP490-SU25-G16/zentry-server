using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    public Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Device entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Update(Device entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(Device entity)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
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
}
