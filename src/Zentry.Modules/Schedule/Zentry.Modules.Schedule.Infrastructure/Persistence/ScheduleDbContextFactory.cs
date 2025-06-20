using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Zentry.Modules.Schedule.Infrastructure.Persistence;

public class ScheduleDbContextFactory : IDesignTimeDbContextFactory<ScheduleDbContext>
{
    public ScheduleDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ScheduleDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for development
            connectionString = "Host=localhost;Database=ZentryDb;Username=postgres;Password=postgres";
        }

        optionsBuilder.UseNpgsql(connectionString,
            b => b.MigrationsAssembly("Zentry.Modules.Schedule.Infrastructure"));

        return new ScheduleDbContext(optionsBuilder.Options);
    }
}
