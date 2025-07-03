using Microsoft.Extensions.DependencyInjection;

namespace Zentry.Modules.DeviceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}