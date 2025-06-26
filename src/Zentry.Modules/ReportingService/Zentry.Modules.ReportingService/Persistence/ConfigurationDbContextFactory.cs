using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Zentry.Modules.ReportingService.Persistence;

public class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
{
    public ReportingDbContext CreateDbContext(string[] args)
    {
        var reporting = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();
        var connectionString = reporting.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            // Fallback connection string for development
            connectionString = "Host=localhost;Database=ZentryDb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString,
            b => b.MigrationsAssembly("Zentry.Modules.ReportingService"));

        return new ReportingDbContext(optionsBuilder.Options);
    }
}
