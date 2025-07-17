using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.SharedKernel.Contracts.Messages;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class ProcessScanDataMessageConsumer(
    ILogger<ProcessScanDataMessageConsumer> logger,
    IServiceScopeFactory serviceScopeFactory)
    : IConsumer<ProcessScanDataMessage>
{
    // To resolve scoped services like DbContexts

    public async Task Consume(ConsumeContext<ProcessScanDataMessage> context)
    {
        var message = context.Message;
        logger.LogInformation("MassTransit Consumer: Received scan data for RequestId: {RequestId}, Session: {SessionId}, Student: {StudentId}",
            message.RequestId, message.SessionId, message.StudentId);

        try
        {
            // Create a new service scope for each message consumption.
            // This is crucial for resolving scoped services (like DbContext, etc.) correctly.
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<IAttendanceProcessorService>();

                // Pass the message and CancellationToken from the ConsumeContext
                await processor.ProcessBluetoothScanData(message, context.CancellationToken);
            }

            logger.LogInformation("MassTransit Consumer: Successfully processed scan data for RequestId: {RequestId}.", message.RequestId);
            // MassTransit automatically Acknowledges (ACKs) the message upon successful completion of Consume method.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MassTransit Consumer: Error processing scan data for RequestId {RequestId}. Message will be retried or moved to error queue.", message.RequestId);
            // Throwing the exception allows MassTransit's retry/dead-lettering policies to take effect.
            throw;
        }
    }
}
