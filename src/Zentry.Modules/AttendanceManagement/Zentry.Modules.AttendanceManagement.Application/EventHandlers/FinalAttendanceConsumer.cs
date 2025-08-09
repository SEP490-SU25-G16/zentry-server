using System.Globalization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class FinalAttendanceConsumer(
    ILogger<FinalAttendanceConsumer> logger,
    IStudentTrackRepository studentTrackRepository,
    IRoundRepository roundRepository,
    IAttendanceRecordRepository attendanceRecordRepository,
    IRedisService redisService)
    : IConsumer<SessionFinalAttendanceToProcess>
{
    private const double AttendanceThresholdPercentage = 75.0;

    public async Task Consume(ConsumeContext<SessionFinalAttendanceToProcess> context)
    {
        var sessionId = context.Message.SessionId;
        var actualRoundsCount = context.Message.ActualRoundsCount;
        var processingKey = $"final_processing:{sessionId}";

        logger.LogInformation(
            "Received SessionFinalAttendanceToProcess for Session {SessionId} with {ActualRounds} actual rounds at {Timestamp}",
            sessionId, actualRoundsCount, context.Message.Timestamp);

        try
        {
            // Implement idempotency check
            var isProcessing = await redisService.SetAsync(processingKey, "processing", TimeSpan.FromMinutes(10));

            if (!isProcessing)
            {
                logger.LogInformation("Final processing already in progress for session {SessionId}, skipping",
                    sessionId);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error acquiring processing lock for session {SessionId}. Aborting.", sessionId);
            // Re-throw to allow MassTransit's retry mechanism to handle this.
            throw;
        }

        try
        {
            await ProcessFinalAttendance(sessionId, actualRoundsCount, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in final attendance processing for session {SessionId}", sessionId);
            throw;
        }
        finally
        {
            // Always attempt to remove the processing lock, regardless of success or failure.
            try
            {
                await redisService.SetAsync($"{processingKey}:completed",
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    TimeSpan.FromHours(24));
                await redisService.RemoveAsync(processingKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error releasing processing lock for session {SessionId}", sessionId);
            }
        }
    }

    private async Task ProcessFinalAttendance(Guid sessionId, int actualRoundsCount,
        CancellationToken cancellationToken)
    {
        if (actualRoundsCount == 0)
        {
            logger.LogWarning(
                "No actual rounds completed for Session {SessionId}. Skipping final attendance calculation.",
                sessionId);
            return;
        }

        IReadOnlyList<StudentTrack> relevantStudentTracks;
        List<Round> completedRounds;
        List<Round> finalizedRounds;

        try
        {
            var allRounds = await roundRepository.GetRoundsBySessionIdAsync(sessionId, cancellationToken);
            completedRounds = allRounds.Where(r => Equals(r.Status, RoundStatus.Completed)).ToList();
            finalizedRounds = allRounds.Where(r => Equals(r.Status, RoundStatus.Finalized)).ToList();

            relevantStudentTracks =
                await studentTrackRepository.GetStudentTracksBySessionIdAsync(sessionId, cancellationToken);

            if (relevantStudentTracks == null)
            {
                logger.LogError("GetStudentTracksBySessionIdAsync returned null for Session {SessionId}", sessionId);
                throw new InvalidOperationException($"Student tracks not found for session {sessionId}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching initial data for session {SessionId}. Aborting process.", sessionId);
            throw; // Re-throw to indicate a critical failure.
        }

        logger.LogInformation(
            "Session {SessionId} status: {CompletedRounds} completed rounds, {FinalizedRounds} finalized rounds, {ActualRounds} rounds used for calculation",
            sessionId, completedRounds.Count, finalizedRounds.Count, actualRoundsCount);

        if (relevantStudentTracks.Count == 0)
        {
            logger.LogInformation(
                "No relevant StudentTracks found for Session {SessionId}. No final attendance records to create/update.",
                sessionId);
            return;
        }

        var attendanceRecordsToProcess = new List<AttendanceRecord>();

        foreach (var studentTrack in relevantStudentTracks)
            try
            {
                var attendedCompletedRounds = studentTrack.Rounds
                    .Count(rp => completedRounds.Any(r => r.Id == rp.RoundId) && rp.IsAttended);

                var percentage = 0.0;
                if (actualRoundsCount > 0) percentage = (double)attendedCompletedRounds / actualRoundsCount * 100.0;

                logger.LogDebug(
                    "Calculated attendance for Student {StudentId} in Session {SessionId}: {Percentage:F2}% (Attended {AttendedRounds} / Actual {ActualRounds} rounds).",
                    studentTrack.Id, sessionId, percentage, attendedCompletedRounds, actualRoundsCount);

                var existingAttendanceRecord =
                    await attendanceRecordRepository.GetByUserIdAndSessionIdAsync(studentTrack.StudentId, sessionId,
                        cancellationToken);

                var newStatus = percentage >= AttendanceThresholdPercentage
                    ? AttendanceStatus.Present
                    : AttendanceStatus.Absent;

                if (existingAttendanceRecord is null)
                {
                    existingAttendanceRecord =
                        AttendanceRecord.Create(studentTrack.StudentId, sessionId, newStatus, false, percentage);
                    logger.LogInformation(
                        "Creating new AttendanceRecord for Student {StudentId} in Session {SessionId}. Status: {Status}, Percentage: {Percentage:F2}%",
                        studentTrack.Id, sessionId, newStatus, percentage);
                }
                else
                {
                    if (!Equals(existingAttendanceRecord.Status, newStatus) ||
                        Math.Abs(existingAttendanceRecord.PercentageAttended - percentage) > 0.01)
                    {
                        existingAttendanceRecord.Update(newStatus, false, null, percentage);
                        logger.LogInformation(
                            "Updating existing AttendanceRecord for Student {StudentId} in Session {SessionId}. New Status: {NewStatus}, New Percentage: {NewPercentage:F2}%",
                            studentTrack.StudentId, sessionId, newStatus, percentage);
                    }
                    else
                    {
                        logger.LogDebug(
                            "AttendanceRecord for Student {StudentId} in Session {SessionId} remains unchanged.",
                            studentTrack.StudentId, sessionId);
                    }
                }

                attendanceRecordsToProcess.Add(existingAttendanceRecord);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing student track for student {StudentId} in session {SessionId}. Skipping this student.",
                    studentTrack.Id, sessionId);
                // Continue to the next student
            }

        try
        {
            // Batch save all records
            foreach (var record in attendanceRecordsToProcess)
                await attendanceRecordRepository.AddOrUpdateAsync(record, cancellationToken);

            await attendanceRecordRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving attendance records for session {SessionId}. Aborting.", sessionId);
            throw;
        }

        logger.LogInformation(
            "Finished processing final attendance for Session {SessionId}. Created/Updated {Count} attendance records based on {ActualRounds} actual rounds.",
            sessionId, attendanceRecordsToProcess.Count, actualRoundsCount);
    }
}