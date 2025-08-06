using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Helpers;

namespace Zentry.Modules.ScheduleManagement.Application;

public static class DependencyCollection
{
    public static IServiceCollection AddScheduleApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserScheduleService, UserScheduleService>();
        services.AddScoped<IFileProcessor<ScheduleImportDto>, ScheduleFileProcessor>();
        return services;
    }
}
