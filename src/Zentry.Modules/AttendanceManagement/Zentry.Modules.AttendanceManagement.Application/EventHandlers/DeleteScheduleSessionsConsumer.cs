using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class DeleteScheduleSessionsConsumer(
    ILogger<DeleteScheduleSessionsConsumer> logger,
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository)
    : IConsumer<ScheduleDeletedMessage>
{
    public async Task Consume(ConsumeContext<ScheduleDeletedMessage> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received ScheduleDeletedMessage for ScheduleId: {ScheduleId}.",
            message.ScheduleId);

        try
        {
            // Bước 1: Lấy tất cả sessions liên quan đến schedule đã xóa
            var sessionsToDelete =
                await sessionRepository.GetSessionsByScheduleIdAsync(message.ScheduleId, context.CancellationToken);

            if (!sessionsToDelete.Any())
            {
                logger.LogInformation("No sessions found for ScheduleId: {ScheduleId}. No action needed.",
                    message.ScheduleId);
                return;
            }

            // Bước 2: Lấy tất cả rounds liên quan đến các sessions đó
            var sessionIds = sessionsToDelete.Select(s => s.Id).ToList();
            var roundsToDelete =
                await roundRepository.GetRoundsBySessionIdsAsync(sessionIds, context.CancellationToken);

            if (roundsToDelete.Count != 0)
            {
                await roundRepository.DeleteRangeAsync(roundsToDelete, context.CancellationToken);
                logger.LogInformation("Successfully deleted {NumRounds} rounds for ScheduleId: {ScheduleId}.",
                    roundsToDelete.Count, message.ScheduleId);
            }

            // Bước 4: Xóa sessions
            await sessionRepository.DeleteRangeAsync(sessionsToDelete, context.CancellationToken);
            logger.LogInformation("Successfully deleted {NumSessions} sessions for ScheduleId: {ScheduleId}.",
                sessionsToDelete.Count, message.ScheduleId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error processing ScheduleDeletedMessage for ScheduleId {ScheduleId}.",
                message.ScheduleId);
            throw;
        }
    }
}