// Trong Zentry.Modules.AttendanceManagement.Application.EventHandlers

using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Exceptions;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class UpdateSessionConsumer(
    ILogger<UpdateSessionConsumer> logger,
    ISessionRepository sessionRepository)
    : IConsumer<ScheduleUpdatedMessage>
{
    public async Task Consume(ConsumeContext<ScheduleUpdatedMessage> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received ScheduleUpdatedMessage for ScheduleId: {ScheduleId}.",
            message.ScheduleId);

        try
        {
            var sessions =
                await sessionRepository.GetSessionsByScheduleIdAsync(message.ScheduleId, context.CancellationToken);

            if (sessions.Count == 0)
            {
                logger.LogWarning("No sessions found for ScheduleId: {ScheduleId} to update.", message.ScheduleId);
                return;
            }

            var sessionsToUpdate = new List<Session>();

            foreach (var session in sessions)
            {
                if (Equals(session.Status, SessionStatus.Active) || Equals(session.Status, SessionStatus.Completed))
                {
                    logger.LogWarning(
                        "Skipping session {SessionId} for update as its status is {Status}.",
                        session.Id, session.Status);
                    continue;
                }

                var newStartTime = message.StartTime;
                var newEndTime = message.EndTime;

                if (newStartTime.HasValue && newEndTime.HasValue)
                {
                    var sessionDate = DateOnly.FromDateTime(session.StartTime.ToVietnamLocalTime());
                    var newLocalStartTime = sessionDate.ToDateTime(newStartTime.Value);
                    var newLocalEndTime = sessionDate.ToDateTime(newEndTime.Value);

                    session.Update(
                        startTime: newLocalStartTime.ToUtcFromVietnamLocalTime(),
                        endTime: newLocalEndTime.ToUtcFromVietnamLocalTime()
                    );
                }

                sessionsToUpdate.Add(session);
            }

            if (sessionsToUpdate.Count != 0)
            {
                await sessionRepository.UpdateRangeAsync(sessionsToUpdate, context.CancellationToken);
                await sessionRepository.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Successfully updated {NumSessions} sessions for ScheduleId: {ScheduleId}.",
                    sessionsToUpdate.Count, message.ScheduleId);
            }
            else
            {
                logger.LogInformation("No sessions to update for ScheduleId: {ScheduleId}.", message.ScheduleId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error processing ScheduleUpdatedMessage for ScheduleId {ScheduleId}.",
                message.ScheduleId);
            throw;
        }
    }
}
