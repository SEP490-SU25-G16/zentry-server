using Zentry.Modules.DeviceManagement.Dtos;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Constants.Device;

namespace Zentry.Modules.DeviceManagement.Abstractions;

public interface IDeviceRepository : IRepository<Device, Guid>
{
    Task<List<Device>> GetByIdsAsync(List<Guid> deviceIds, CancellationToken cancellationToken);
    Task<IEnumerable<Device>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
    Task<Device?> GetByDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken);
    Task<Guid?> GetActiveDeviceByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<Device>> GetActiveDevicesByUserIdsAsync(List<Guid> userIds, CancellationToken cancellationToken);
    Task<List<Device>> GetUserIdsByDeviceIdsAsync(List<Guid> deviceIds, CancellationToken cancellationToken);
    Task AddAsync(Device device);

    Task<Device?> GetByMacAddressAsync(string macAddress, CancellationToken cancellationToken);

    Task<(Guid DeviceId, Guid UserId)?> GetDeviceAndUserIdByMacAddressAsync(string macAddress,
        CancellationToken cancellationToken);

    Task<List<Device>> GetActiveDevicesByMacAddressesAsync(List<string> macAddresses,
        CancellationToken cancellationToken);

    Task<Dictionary<string, (Guid DeviceId, Guid UserId)>> GetDeviceAndUserIdsByMacAddressesAsync(
        List<string> macAddresses, CancellationToken cancellationToken);

    Task<(IEnumerable<DeviceListItemDto> Devices, int TotalCount)> GetDevicesAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        Guid? userId,
        DeviceStatus? status,
        CancellationToken cancellationToken);

    Task<Device?> GetUserActiveDeviceAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> CountAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Device?> GetActiveDeviceForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<Device?> GetPendingDeviceForUserAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken);
}
