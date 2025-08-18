using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.AttendanceManagement.Application.Services;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.ScheduleManagement.Application.Integration;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.AttendanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAttendanceApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceCalculationService, AttendanceCalculationService>();
        services.AddScoped<IAttendancePersistenceService, AttendancePersistenceService>();

        // Register integration query handlers
        services
            .AddScoped<IQueryHandler<GetClassSectionByScheduleIdIntegrationQuery,
                GetClassSectionByScheduleIdIntegrationResponse>, GetClassSectionByScheduleQueryHandler>();
        services
            .AddScoped<IQueryHandler<GetStudentIdsByClassSectionIdIntegrationQuery,
                GetStudentIdsByClassSectionIdIntegrationResponse>, GetStudentIdsByClassSectionIdQueryHandler>();

        return services;
    }
}