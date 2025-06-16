using Application.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceManagementInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DeviceManagementDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DeviceManagementConnection")));

        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}