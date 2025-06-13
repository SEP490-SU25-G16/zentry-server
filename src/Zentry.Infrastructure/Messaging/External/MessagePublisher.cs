using RabbitMQ.Client;

namespace Zentry.Infrastructure.Messaging.External;

public class MessagePublisher
{
    private readonly IConnection _connection;

    public void Publish(string queue, string message)
    {
    }
}