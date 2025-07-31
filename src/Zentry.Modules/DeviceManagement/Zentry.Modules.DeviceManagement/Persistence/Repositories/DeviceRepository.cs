using Microsoft.EntityFrameworkCore;
using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Constants.Device;

// Thêm using này để truy cập MacAddress Value Object

namespace Zentry.Modules.DeviceManagement.Persistence.Repositories;

public class DeviceRepository(DeviceDbContext dbContext) : IDeviceRepository
{
    public async Task<List<Device>> GetByIdsAsync(List<Guid> deviceIds, CancellationToken cancellationToken)
    {
        return await dbContext.Devices
            .Where(d => deviceIds.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> GetActiveDeviceByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Devices
            .AsNoTracking()
            .Where(d => d.UserId == userId && d.Status == DeviceStatus.Active)
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Device>> GetActiveDevicesByUserIdsAsync(List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Devices
            .AsNoTracking()
            .Where(d => userIds.Contains(d.UserId) && d.Status == DeviceStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Device>> GetUserIdsByDeviceIdsAsync(List<Guid> deviceIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Devices
            .AsNoTracking() // Tối ưu hóa cho các query chỉ đọc
            .Where(d => deviceIds.Contains(d.Id) && d.Status == DeviceStatus.Active)
            .ToListAsync(cancellationToken);
    }

    // Thêm các method liên quan đến MAC address
    public async Task<Device?> GetByMacAddressAsync(string macAddress, CancellationToken cancellationToken)
    {
        // Sử dụng MacAddress.Create để đảm bảo MAC address được chuẩn hóa
        // và so sánh với giá trị string của MacAddress trong DB.
        // EF Core sẽ sử dụng implicit operator string hoặc cơ chế HasConversion.
        var macAddressObject = MacAddress.Create(macAddress);

        return await dbContext.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => (string)d.MacAddress == macAddressObject.Value, cancellationToken);
        // ^^^ Dùng explicit cast để đảm bảo EF Core dịch đúng
    }

    public async Task<(Guid DeviceId, Guid UserId)?> GetDeviceAndUserIdByMacAddressAsync(string macAddress,
        CancellationToken cancellationToken)
    {
        var macAddressObject = MacAddress.Create(macAddress);

        var result = await dbContext.Devices
            .AsNoTracking()
            .Where(d => (string)d.MacAddress == macAddressObject.Value && d.Status == DeviceStatus.Active)
            .Select(d => new { d.Id, d.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        return result != null ? (result.Id, result.UserId) : null;
    }

    public async Task<List<Device>> GetActiveDevicesByMacAddressesAsync(List<string> macAddresses,
        CancellationToken cancellationToken)
    {
        // Chuẩn hóa tất cả MAC addresses đầu vào thành list các string
        var normalizedMacs = macAddresses.Select(m => MacAddress.Create(m).Value).ToList();

        return await dbContext.Devices
            .AsNoTracking()
            .Where(d => normalizedMacs.Contains((string)d.MacAddress) && d.Status == DeviceStatus.Active)
            //                                  ^^^ Dùng explicit cast
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, (Guid DeviceId, Guid UserId)>> GetDeviceAndUserIdsByMacAddressesAsync(
        List<string> macAddresses, CancellationToken cancellationToken)
    {
        var normalizedMacs = macAddresses.Select(m => MacAddress.Create(m).Value).ToList();

        var results = await dbContext.Devices
            .AsNoTracking()
            .Where(d => normalizedMacs.Contains((string)d.MacAddress) && d.Status == DeviceStatus.Active)
            //                                  ^^^ Dùng explicit cast
            .Select(d => new { MacValue = (string)d.MacAddress, d.Id, d.UserId }) // Lấy giá trị string của MacAddress
            .ToListAsync(cancellationToken);

        return results.ToDictionary(
            r => r.MacValue, // Dùng MacValue đã được Select ra
            r => (r.Id, r.UserId)
        );
    }

    public async Task AddAsync(Device device)
    {
        await dbContext.Devices.AddAsync(device);
    }

    public async Task AddRangeAsync(IEnumerable<Device> entities, CancellationToken cancellationToken)
    {
        await dbContext.Devices.AddRangeAsync(entities, cancellationToken);
    }

    public Task DeleteAsync(Device entity, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use Soft delete please.");
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Devices.ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task AddAsync(Device entity, CancellationToken cancellationToken)
    {
        await dbContext.Devices.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Device entity, CancellationToken cancellationToken)
    {
        dbContext.Devices.Update(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        // Assuming AccountId maps to UserId for devices in this dbContext. Adjust if separate.
        return await dbContext.Devices
            .Where(d => d.UserId == accountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken)
    {
        // Assuming DeviceToken is stored directly or as an owned type's value.
        // Make sure your EF Core setting for DeviceToken allows this query.
        // Cần đảm bảo DeviceToken.Create(v) trong Configuration ánh xạ đúng cách
        return await dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceToken.Value == deviceToken, cancellationToken);
    }
}