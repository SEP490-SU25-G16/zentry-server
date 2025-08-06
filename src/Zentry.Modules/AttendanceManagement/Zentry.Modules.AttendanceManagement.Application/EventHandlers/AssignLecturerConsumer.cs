using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class AssignLecturerConsumer(
    ILogger<AssignLecturerConsumer> logger,
    ISessionRepository sessionRepository,
    IMediator mediator,
    IPublishEndpoint publishEndpoint) : IConsumer<AssignLecturerMessage>
{
    public async Task Consume(ConsumeContext<AssignLecturerMessage> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "Received AssignLecturerMessage for ClassSectionId: {ClassSectionId} and LecturerId: {LecturerId}.",
            message.ClassSectionId, message.LecturerId);

        var getSchedulesQuery = new GetSchedulesByClassSectionIdIntegrationQuery(message.ClassSectionId);
        var schedulesResponse = await mediator.Send(getSchedulesQuery, context.CancellationToken);

        if (schedulesResponse.Schedules.Count == 0)
        {
            logger.LogWarning("No schedules found for ClassSectionId {ClassSectionId}. Skipping.",
                message.ClassSectionId);
            return;
        }

        var scheduleIds = schedulesResponse.Schedules.Select(s => s.ScheduleId).ToList();

        var sessions = await sessionRepository.GetSessionsByScheduleIdsAsync(scheduleIds, context.CancellationToken);

        foreach (var session in sessions)
        {
            session.AssignLecturer(message.LecturerId);
            await sessionRepository.UpdateAsync(session, context.CancellationToken);

            await publishEndpoint.Publish(new GenerateScheduleWhitelistMessage(
                session.ScheduleId,
                message.ClassSectionId,
                message.LecturerId
            ), context.CancellationToken);
        }

        await sessionRepository.SaveChangesAsync(context.CancellationToken);
    }
}
