using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Zentry.Infrastructure.Messaging.External;
using Zentry.Modules.Reporting.Persistence;
using Zentry.Modules.Reporting.Persistence.Configurations;

namespace Zentry.Modules.Reporting;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Đảm bảo DbContextOptions<ReportingDbContext> được typed đúng
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.Reporting")
            ));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
