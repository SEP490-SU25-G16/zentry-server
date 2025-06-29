using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Zentry.Infrastructure.Messaging.External;
using Zentry.Modules.NotificationService.Persistence.Configurations;
using Zentry.Modules.NotificationService.Persistence.Data;
using Zentry.Modules.NotificationService.Persistence.Repository;

namespace Zentry.Modules.NotificationService;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoConnectionString = configuration["MongoDB_ConnectionString"] ??
                                    throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IMongoClient>(s => new MongoClient(mongoConnectionString));

        services.AddSingleton(s =>
        {
            var mongoClient = s.GetRequiredService<IMongoClient>();
            var database = mongoClient.GetDatabase("zentry");
            NotificationConfiguration.Configure(database);
            NotificationSeed.SeedData(database);
            return database;
        });

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddSingleton<IMessagePublisher, MessagePublisher>();

        return services;
    }
}