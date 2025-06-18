using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Messaging.External;

namespace Zentry.BackgroundProcessor.Workers;

public class AttendanceProcessingWorker(ILogger<AttendanceProcessingWorker> logger, MessagePublisher messagePublisher)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AttendanceProcessingWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            const string message = "{\"rssi\": -65, \"timestamp\": \"2025-06-13T17:00:00Z\"}";
            await messagePublisher.PublishAsync("rssi_queue", message, stoppingToken);
            logger.LogInformation("Sent RSSI message to queue: {Message}", message);

            await Task.Delay(5000, stoppingToken); // Chờ 5 giây để mô phỏng
        }
    }
}