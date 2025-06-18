using Zentry.Modules.DeviceManagement.Domain.Entities;

namespace Zentry.Modules.DeviceManagement.Application.Abstractions;

public interface IDeviceRepository
{
    Task<Device> GetByIdAsync(Guid deviceId, CancellationToken cancellationToken);
    Task<IEnumerable<Device>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
    Task<Device> GetByDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken);
    Task AddAsync(Device device, CancellationToken cancellationToken);
    Task UpdateAsync(Device device, CancellationToken cancellationToken);
    Task DeleteAsync(Device device, CancellationToken cancellationToken);
}