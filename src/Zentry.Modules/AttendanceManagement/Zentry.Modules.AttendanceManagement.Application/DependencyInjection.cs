using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;

namespace Zentry.Modules.AttendanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAttendanceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceProcessorService, AttendanceProcessorService>();

        return services;
    }
}
