using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.NotificationService.Application.EventHandlers;
using Zentry.Modules.NotificationService.Application.Services;
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
            options.UseSqlServer(configuration.GetConnectionString("Database")));
            // Lưu ý: Thay "Database" bằng tên connection string thực tế của bạn

        // 2. Đăng ký Repositories và Services
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>(); // Tạm thời dùng mock
        services.AddScoped<IFcmSender, FcmSender>();
        services.AddScoped<INotificationSender, NotificationSender>();

        // 3. Đăng ký MediatR handlers (nếu có)
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        // 4. Cấu hình MassTransit
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<NotificationCreatedEventHandler>();

            busConfigurator.UsingRabbitMq((context, mqConfigurator) =>
            {
                mqConfigurator.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:User"]);
                    h.Password(configuration["RabbitMQ:Password"]);
                });
                
                // Cấu hình endpoint để nhận event
                mqConfigurator.ReceiveEndpoint("notification-created-event", e =>
                {
                    e.ConfigureConsumer<NotificationCreatedEventHandler>(context);
                    // Cấu hình retry policy
                    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));
                });
            });
        });

        return services;
    }
}