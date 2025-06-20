using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Zentry.Modules.Schedule.Application.Abstractions;
using Zentry.Modules.Schedule.Infrastructure.Persistence;
using Zentry.Modules.Schedule.Infrastructure.Repositories;

namespace Zentry.Modules.Schedule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ScheduleDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.Schedule.Infrastructure")
            ));

        services.AddScoped<IScheduleRepository, ScheduleRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
