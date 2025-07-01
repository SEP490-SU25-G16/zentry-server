using Microsoft.EntityFrameworkCore;
using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Domain.Entities;
using Zentry.Modules.DeviceManagement.Domain.Enums;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;
// Assuming DeviceDbContext is here
// For Entity Framework Core methods
// For DeviceStatus enum

// For DeviceToken

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

    public async Task AddAsync(Device device) //
    {
        // Adds a new Device entity to the DbContext.
        await dbContext.Devices.AddAsync(device); //
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) //
    {
        // Saves all pending changes in the DbContext to the database.
        await dbContext.SaveChangesAsync(cancellationToken); //
    }

    // --- Other methods from IDeviceRepository (implemented as per your code) ---
    // You would implement these fully based on your specific requirements.
    // For now, I'll provide basic working implementations or keep NotImplementedException if they are not directly part of Register Device.

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
        await dbContext.Devices.AddAsync(entity);
    }

    public void Update(Device entity)
    {
        dbContext.Devices.Update(entity);
    }

    public void Delete(Device entity)
    {
        dbContext.Devices.Remove(entity);
    }

    public async Task<IEnumerable<Device>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        // Assuming AccountId maps to UserId for devices in this context. Adjust if separate.
        return await dbContext.Devices
            .Where(d => d.UserId == accountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken)
    {
        // Assuming DeviceToken is stored directly or as an owned type's value.
        // Make sure your EF Core configuration for DeviceToken allows this query.
        return await dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceToken.Value == deviceToken, cancellationToken);
    }
}
