using Microsoft.EntityFrameworkCore;
using Zentry.Modules.DeviceManagement.Domain.Entities;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Persistence;

public class DeviceManagementDbContext(DbContextOptions<DeviceManagementDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
