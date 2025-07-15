using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Services;

namespace Zentry.Modules.ScheduleManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IUserScheduleService, UserScheduleService>();

        return services;
    }
}
