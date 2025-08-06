using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IUserScheduleService, UserScheduleService>();
        services.AddScoped<IFileProcessor<ScheduleImportDto>, ScheduleFileProcessor>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
