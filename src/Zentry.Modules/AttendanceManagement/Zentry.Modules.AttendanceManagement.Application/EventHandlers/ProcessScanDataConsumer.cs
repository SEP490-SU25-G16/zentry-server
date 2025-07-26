using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class ProcessScanDataConsumer(
    ILogger<ProcessScanDataConsumer> logger,
    IServiceScopeFactory serviceScopeFactory,
    IAttendanceProcessorService processor)
    : IConsumer<ProcessScanDataMessage>
{
    // To resolve scoped services like DbContexts

    public async Task Consume(ConsumeContext<ProcessScanDataMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received scan data for SessionId: {SessionId}, Device: {DeviceId}, Submitter: {SubmitterUserId}",
            message.SessionId, message.DeviceId, message.SubmitterUserId);

        try
        {
            await processor.ProcessBluetoothScanData(message, consumeContext.CancellationToken);

            logger.LogInformation("MassTransit Consumer: Successfully processed scan data for SessionId: {SessionId}.",
                message.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error processing scan data for SessionId {SessionId}. Message will be retried or moved to error queue.",
                message.SessionId);
            throw;
        }
    }
}
