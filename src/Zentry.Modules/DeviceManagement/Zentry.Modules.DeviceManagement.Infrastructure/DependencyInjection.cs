using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;
using Zentry.Modules.DeviceManagement.Infrastructure.Repositories;

namespace Zentry.Modules.DeviceManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceManagementInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DeviceManagementDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.DeviceManagement.Infrastructure")
            ));

        services.AddScoped<IDeviceRepository, DeviceRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}