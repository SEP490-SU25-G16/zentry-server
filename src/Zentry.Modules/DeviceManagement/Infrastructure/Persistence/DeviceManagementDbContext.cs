using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class DeviceManagementDbContext(DbContextOptions<DeviceManagementDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceManagementDbContext).Assembly);
    }
}