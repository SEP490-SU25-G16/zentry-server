using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.SeedData;
using Zentry.Modules.ConfigurationManagement.Services;

namespace Zentry.Modules.ConfigurationManagement;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigurationInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ConfigurationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.ConfigurationManagement")
            ));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IAttributeService, AttributeService>();
        services.AddScoped<ConfigurationDbSeeder>();
        services.AddHostedService<ConfigurationDbMigrationService>();
        return services;
    }
}