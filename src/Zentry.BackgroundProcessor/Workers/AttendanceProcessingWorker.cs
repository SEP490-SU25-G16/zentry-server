using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Messaging.External;

namespace Zentry.BackgroundProcessor.Workers;

public class AttendanceProcessingWorker : BackgroundService
{
    private readonly ILogger<AttendanceProcessingWorker> _logger;
    private readonly MessagePublisher _messagePublisher;

    public AttendanceProcessingWorker(ILogger<AttendanceProcessingWorker> logger, MessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AttendanceProcessingWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = "{\"rssi\": -65, \"timestamp\": \"2025-06-13T17:00:00Z\"}";
            _messagePublisher.Publish("rssi_queue", message);
            _logger.LogInformation("Sent RSSI message to queue: {Message}", message);

            await Task.Delay(5000, stoppingToken); // Chờ 5 giây để mô phỏng
        }
    }
}