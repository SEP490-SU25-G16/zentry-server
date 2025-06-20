using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.Attendance.Application.Abstractions;
using Zentry.Modules.Attendance.Infrastructure.Persistence;
using Zentry.Modules.Attendance.Infrastructure.Repositories;

namespace Zentry.Modules.Attendance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAttendanceInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.Attendance.Infrastructure")
            ));

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
