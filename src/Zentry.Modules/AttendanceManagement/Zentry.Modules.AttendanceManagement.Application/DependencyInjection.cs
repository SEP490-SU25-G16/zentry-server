using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Infrastructure.Services;

namespace Zentry.Modules.AttendanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAttendanceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IUserAttendanceService, UserAttendanceService>();
        services.AddScoped<IAppConfigurationService, AppConfigurationService>();
        services.AddScoped<IScheduleService, ScheduleService>();

        return services;
    }
}
