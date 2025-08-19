using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class SessionCreatedAttendanceConsumer(
    ILogger<SessionCreatedAttendanceConsumer> logger,
    IAttendanceRecordRepository attendanceRecordRepository,
    ISessionRepository sessionRepository,
    IMediator mediator)
    : IConsumer<SessionCreatedMessage>
{
    public async Task Consume(ConsumeContext<SessionCreatedMessage> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "Received SessionCreatedMessage for SessionId: {SessionId}, ScheduleId: {ScheduleId}",
            message.SessionId, message.ScheduleId);

        try
        {
            // Verify session exists
            var session = await sessionRepository.GetByIdAsync(message.SessionId, context.CancellationToken);
            if (session == null)
            {
                logger.LogWarning("Session {SessionId} not found. Skipping attendance record creation.",
                    message.SessionId);
                return;
            }

            // Get ClassSection information from ScheduleId
            var classSectionResponse = await mediator.Send(
                new GetClassSectionByScheduleIdIntegrationQuery(message.ScheduleId),
                context.CancellationToken);

            if (classSectionResponse.ClassSectionId == Guid.Empty)
            {
                logger.LogWarning(
                    "No ClassSection found for ScheduleId {ScheduleId}. Cannot create attendance records for SessionId {SessionId}",
                    message.ScheduleId, message.SessionId);
                return;
            }

            // Get all enrolled students for this class section
            var studentIdsResponse = await mediator.Send(
                new GetStudentIdsByClassSectionIdIntegrationQuery(classSectionResponse.ClassSectionId),
                context.CancellationToken);

            if (studentIdsResponse.StudentIds.Count == 0)
            {
                logger.LogInformation(
                    "No enrolled students found for ClassSectionId {ClassSectionId}, SessionId {SessionId}",
                    classSectionResponse.ClassSectionId, message.SessionId);
                throw new BusinessRuleException(nameof(SessionCreatedAttendanceConsumer),
                    "Need student before create attendance records");
            }

            // Check if attendance records already exist (idempotency)
            var existingRecords = await attendanceRecordRepository
                .GetAttendanceRecordsBySessionIdAsync(message.SessionId, context.CancellationToken);

            var existingStudentIds = existingRecords.Select(r => r.StudentId).ToHashSet();
            var studentsNeedingRecords = studentIdsResponse.StudentIds
                .Where(studentId => !existingStudentIds.Contains(studentId))
                .ToList();

            if (studentsNeedingRecords.Count == 0)
            {
                logger.LogInformation(
                    "All attendance records already exist for SessionId {SessionId}. No new records to create.",
                    message.SessionId);
                return;
            }

            // Create Future attendance records for students who don't have records yet
            var futureAttendanceRecords = studentsNeedingRecords.Select(studentId =>
                    AttendanceRecord.Create(
                        studentId,
                        message.SessionId,
                        AttendanceStatus.Future,
                        false,
                        0.0))
                .ToList();

            // Save all future attendance records
            await attendanceRecordRepository.AddRangeAsync(futureAttendanceRecords, context.CancellationToken);
            await attendanceRecordRepository.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation(
                "Successfully created {Count} Future attendance records for SessionId {SessionId} (ClassSection: {ClassSectionId})",
                futureAttendanceRecords.Count, message.SessionId, classSectionResponse.ClassSectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error creating future attendance records for SessionId {SessionId}, ScheduleId {ScheduleId}",
                message.SessionId, message.ScheduleId);
            throw;
        }
    }
}