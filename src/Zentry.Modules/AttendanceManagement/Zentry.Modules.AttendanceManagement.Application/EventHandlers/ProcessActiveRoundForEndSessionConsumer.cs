using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class ProcessActiveRoundForEndSessionConsumer(
    ILogger<ProcessActiveRoundForEndSessionConsumer> logger,
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IPublishEndpoint publishEndpoint,
    IRedisService redisService,
    IAttendanceCalculationService attendanceCalculationService,
    IAttendancePersistenceService attendancePersistenceService,
    IMediator mediator)
    : IConsumer<ProcessActiveRoundForEndSessionMessage>
{
    public async Task Consume(ConsumeContext<ProcessActiveRoundForEndSessionMessage> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing active round {ActiveRoundId} for end session {SessionId}",
            message.ActiveRoundId, message.SessionId);

        try
        {
            var session = await sessionRepository.GetByIdAsync(message.SessionId, context.CancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(ProcessActiveRoundForEndSessionConsumer), message.SessionId);

            // Get the active round
            var activeRound = await roundRepository.GetByIdAsync(message.ActiveRoundId, context.CancellationToken);
            if (activeRound is null)
                throw new NotFoundException(nameof(ProcessActiveRoundForEndSessionConsumer), message.ActiveRoundId);

            // Calculate attendance for active round - this will throw if scan data doesn't exist
            logger.LogInformation("Calculating attendance for active round {RoundId}", activeRound.Id);

            var calculationResult = await attendanceCalculationService.CalculateAttendanceForRound(
                session.Id,
                activeRound.Id,
                context.CancellationToken);

            // Persist attendance result
            await attendancePersistenceService.PersistAttendanceResult(
                activeRound,
                calculationResult.AttendedDeviceIds,
                context.CancellationToken);

            // Mark active round as completed
            activeRound.CompleteRound();
            await roundRepository.UpdateAsync(activeRound, context.CancellationToken);

            // Mark pending rounds as Finalized
            foreach (var pendingRoundId in message.PendingRoundIds)
            {
                var pendingRound = await roundRepository.GetByIdAsync(pendingRoundId, context.CancellationToken);
                if (pendingRound is not null)
                {
                    pendingRound.UpdateStatus(RoundStatus.Finalized);
                    await roundRepository.UpdateAsync(pendingRound, context.CancellationToken);
                    logger.LogInformation("Pending round {RoundId} marked as Finalized", pendingRoundId);
                }
            }

            // Save all round changes
            await roundRepository.SaveChangesAsync(context.CancellationToken);

            // Complete session
            session.CompleteSession();
            await sessionRepository.UpdateAsync(session, context.CancellationToken);
            await sessionRepository.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Session {SessionId} status updated to Completed.", session.Id);

            // Clean up Redis
            var activeScheduleKey = $"active_schedule:{session.ScheduleId}";
            await redisService.RemoveAsync($"session:{session.Id}");
            await redisService.RemoveAsync(activeScheduleKey);
            logger.LogInformation("Redis keys for Session {SessionId} and Schedule {ScheduleId} deleted.",
                session.Id, session.ScheduleId);

            // Get total completed rounds for final processing
            var allRounds = await roundRepository.GetRoundsBySessionIdAsync(session.Id, context.CancellationToken);
            var completedRoundsCount = allRounds.Count(r => Equals(r.Status, RoundStatus.Completed));

            // Publish final attendance processing message
            var finalMessage = new SessionFinalAttendanceToProcess
            {
                SessionId = session.Id,
                ActualRoundsCount = completedRoundsCount
            };
            await publishEndpoint.Publish(finalMessage, context.CancellationToken);
            logger.LogInformation(
                "SessionFinalAttendanceToProcess message published for Session {SessionId} with {ActualRounds} actual rounds.",
                session.Id, completedRoundsCount);

            // Send notifications to students about session ending early
            await NotifyStudentsAboutSessionEndingEarly(session, context.CancellationToken);

            logger.LogInformation("Active round {RoundId} processing completed successfully", activeRound.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing active round {ActiveRoundId} for end session {SessionId}. Will retry.",
                message.ActiveRoundId, message.SessionId);

            // Rethrow to trigger retry mechanism
            throw;
        }
    }

    private async Task NotifyStudentsAboutSessionEndingEarly(Session session, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get ClassSectionId from ScheduleId
            var classSectionResponse = await mediator.Send(
                new GetClassSectionByScheduleIdIntegrationQuery(session.ScheduleId), 
                cancellationToken);

            if (classSectionResponse.ClassSectionId == Guid.Empty)
            {
                logger.LogWarning("Could not find ClassSection for Schedule {ScheduleId}, skipping notifications", 
                    session.ScheduleId);
                return;
            }

            // 2. Get all student IDs enrolled in this class section
            var studentIdsResponse = await mediator.Send(
                new GetStudentIdsByClassSectionIdIntegrationQuery(classSectionResponse.ClassSectionId), 
                cancellationToken);

            if (studentIdsResponse.StudentIds.Count == 0)
            {
                logger.LogInformation("No students found for ClassSection {ClassSectionId}, skipping notifications", 
                    classSectionResponse.ClassSectionId);
                return;
            }

            // 3. Create deeplink to StudentScheduleClassDetailFragment
            var deeplink = $"zentry://schedule-detail?scheduleId={session.ScheduleId}&classSectionId={classSectionResponse.ClassSectionId}";

            // 4. Send notifications to all students
            var title = "Tiết học đã kết thúc sớm";
            var body = "Giảng viên đã kết thúc tiết học sớm hơn dự kiến.";

            var notificationTasks = studentIdsResponse.StudentIds.Select(studentId => 
                publishEndpoint.Publish(new NotificationCreatedEvent
                {
                    Title = title,
                    Body = body,
                    RecipientUserId = studentId,
                    Type = NotificationType.All, // Both InApp and Push
                    Data = new Dictionary<string, string>
                    {
                        ["type"] = "SESSION_ENDED_EARLY",
                        ["sessionId"] = session.Id.ToString(),
                        ["scheduleId"] = session.ScheduleId.ToString(),
                        ["classSectionId"] = classSectionResponse.ClassSectionId.ToString(),
                        ["deeplink"] = deeplink,
                        ["action"] = "VIEW_SCHEDULE_DETAIL",
                        ["courseName"] = classSectionResponse.CourseName,
                        ["sectionCode"] = classSectionResponse.SectionCode
                    }
                }, cancellationToken));

            await Task.WhenAll(notificationTasks);

            logger.LogInformation(
                "Session ended early notifications sent to {StudentCount} students for Session {SessionId}",
                studentIdsResponse.StudentIds.Count, session.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send session ended early notifications for Session {SessionId}", 
                session.Id);
            // Don't throw - notification failure shouldn't prevent session from ending
        }
    }
}