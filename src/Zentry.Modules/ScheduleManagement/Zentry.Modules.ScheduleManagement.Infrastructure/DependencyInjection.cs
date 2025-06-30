using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;
using Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

namespace Zentry.Modules.ScheduleManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ScheduleDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.ScheduleManagement.Infrastructure")
            ));
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
