using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ConfigurationManagement.Entities;

namespace Zentry.Modules.ConfigurationManagement.Persistence;

public class ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : DbContext(options)
{
    public DbSet<Setting> Settings { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }
    public DbSet<UserAttribute> UserAttributes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}