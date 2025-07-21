using Microsoft.EntityFrameworkCore;
using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Domain.Entities;
using Zentry.Modules.DeviceManagement.Domain.Enums;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Repositories;

public class DeviceRepository(DeviceDbContext dbContext) : IDeviceRepository
{
    public async Task<Device?> GetActiveDeviceByUserIdAsync(Guid userId)
    {
        // This method is crucial for the "Register Device" use case to check for existing active devices.
        // It fetches a device that belongs to the given userId AND has a status of 'Active'.
        return await dbContext.Devices
            .AsNoTracking() // Use AsNoTracking for read-only operations to improve performance
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Status == DeviceStatus.Active); //
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
        return await dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceToken.Value == deviceToken, cancellationToken);
    }
}