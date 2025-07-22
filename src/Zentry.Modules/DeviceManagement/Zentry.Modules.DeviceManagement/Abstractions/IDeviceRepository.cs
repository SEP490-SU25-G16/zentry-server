using Zentry.Modules.DeviceManagement.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.DeviceManagement.Abstractions;

public interface IDeviceRepository : IRepository<Device, Guid>
{
    Task<IEnumerable<Device>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
    Task<Device?> GetByDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken);
    Task<Device?> GetActiveDeviceByUserIdAsync(Guid userId);
    Task AddAsync(Device device);
}
