using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;
using Zentry.Modules.DeviceManagement.Infrastructure.Repositories;
using Zentry.Modules.DeviceManagement.Infrastructure.Services;

namespace Zentry.Modules.DeviceManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DeviceDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.DeviceManagement.Infrastructure")
            ));

        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();

        return services;
    }
}