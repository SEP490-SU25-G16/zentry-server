using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ConfigurationManagement.Domain.Entities;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure.Persistence;

public class ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : DbContext(options)
{
    public DbSet<Configuration> Configurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}