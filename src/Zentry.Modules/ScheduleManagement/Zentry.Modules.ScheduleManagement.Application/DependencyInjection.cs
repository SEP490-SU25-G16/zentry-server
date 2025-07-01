using Microsoft.Extensions.DependencyInjection;

namespace Zentry.Modules.ScheduleManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}