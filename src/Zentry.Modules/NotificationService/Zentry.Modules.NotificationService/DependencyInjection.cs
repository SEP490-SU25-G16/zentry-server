using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.NotificationService.Application.EventHandlers;
using Zentry.Modules.NotificationService.Application.Services;
using Zentry.Modules.NotificationService.Infrastructure.DeviceTokens;
using Zentry.Modules.NotificationService.Infrastructure.Persistence;
using Zentry.Modules.NotificationService.Infrastructure.Push;
using Zentry.Modules.NotificationService.Infrastructure.Services;

namespace Zentry.Modules.NotificationService;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Cấu hình EF Core
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. Đăng ký Repositories và Services
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeviceTokenRepository, Infrastructure.DeviceTokens.DeviceTokenRepository>(); // Real implementation using DeviceManagement integration
        services.AddScoped<IFcmSender, FcmSender>();
        services.AddScoped<INotificationSender, NotificationSender>();

        // 3. Đăng ký MediatR handlers (nếu có)
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        // 4. Note: MassTransit is configured centrally in Program.cs
        // The NotificationCreatedEventHandler will be automatically discovered

        return services;
    }
}