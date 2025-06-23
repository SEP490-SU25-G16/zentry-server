using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ConfigurationManagement.Application.Abstractions;
using Zentry.Modules.ConfigurationManagement.Infrastructure.Persistence;
using Zentry.Modules.ConfigurationManagement.Infrastructure.Repositories;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigurationInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Đảm bảo DbContextOptions<ConfigurationDbContext> được typed đúng
        services.AddDbContext<ConfigurationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.ConfigurationManagement.Infrastructure")
            ));

        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}