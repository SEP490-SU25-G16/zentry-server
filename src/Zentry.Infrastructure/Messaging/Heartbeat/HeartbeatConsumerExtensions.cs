using MassTransit;

namespace Zentry.Infrastructure.Messaging.Heartbeat;


/// <summary>
/// Extension để đăng ký heartbeat consumer
/// </summary>
public static class HeartbeatConsumerExtensions
{
    public static void AddHeartbeatConsumer(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<HeartbeatConsumer>();
    }

    public static void ConfigureHeartbeatEndpoint(this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("heartbeat_queue", e =>
        {
            e.ConfigureConsumer<HeartbeatConsumer>(context);
            e.Durable = false; // Không cần persistent cho heartbeat
            e.AutoDelete = true; // Tự động xóa khi không dùng
            e.PrefetchCount = 1;
            e.ConcurrentMessageLimit = 1;
        });
    }
}
