using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class FinalAttendanceConsumer(
    ILogger<FinalAttendanceConsumer> logger,
    IStudentTrackRepository studentTrackRepository,
    IRoundRepository roundRepository,
    IAttendanceRecordRepository attendanceRecordRepository)
    : IConsumer<SessionFinalAttendanceToProcess>
{
    private const double AttendanceThresholdPercentage = 75.0;

    public async Task Consume(ConsumeContext<SessionFinalAttendanceToProcess> context)
    {
        var sessionId = context.Message.SessionId;
        var actualRoundsCount = context.Message.ActualRoundsCount;

        logger.LogInformation(
            "Received SessionFinalAttendanceToProcess for Session {SessionId} with {ActualRounds} actual rounds at {Timestamp}. Starting final attendance calculation.",
            sessionId, actualRoundsCount, context.Message.Timestamp);

        if (actualRoundsCount == 0)
        {
            logger.LogWarning("No actual rounds completed for Session {SessionId}. Skipping final attendance calculation.",
                sessionId);
            return;
        }

        var completedRounds = await roundRepository.GetRoundsBySessionIdAsync(sessionId, context.CancellationToken);
        var actualCompletedRounds = completedRounds.Where(r => Equals(r.Status, RoundStatus.Completed)).ToList();
        var finalizedRounds = completedRounds.Where(r => Equals(r.Status, RoundStatus.Finalized)).ToList();

        logger.LogInformation(
            "Session {SessionId} status: {CompletedRounds} completed rounds, {FinalizedRounds} finalized rounds, {ActualRounds} rounds used for calculation",
            sessionId, actualCompletedRounds.Count, finalizedRounds.Count, actualRoundsCount);

        IReadOnlyList<StudentTrack> relevantStudentTracks =
            await studentTrackRepository.GetStudentTracksBySessionIdAsync(sessionId,
                context.CancellationToken);
        if (relevantStudentTracks == null) throw new ArgumentNullException(nameof(relevantStudentTracks));

        if (relevantStudentTracks.Count == 0)
        {
            logger.LogInformation(
                "No relevant StudentTracks found for Session {SessionId}. No final attendance records to create/update.",
                sessionId);
            return;
        }

        var attendanceRecordsToProcess = new List<AttendanceRecord>();

        foreach (var studentTrack in relevantStudentTracks)
        {
            var attendedCompletedRounds = studentTrack.Rounds
                .Count(rp => actualCompletedRounds.Any(r => r.Id == rp.RoundId) && rp.IsAttended);

            // THAY ĐỔI: tính % dựa trên actualRoundsCount thay vì totalRoundsInSession
            var percentage = actualRoundsCount > 0
                ? (double)attendedCompletedRounds / actualRoundsCount * 100.0
                : 0.0;

            logger.LogDebug(
                "Calculated attendance for Student {StudentId} in Session {SessionId}: {Percentage:F2}% (Attended {AttendedRounds} / Actual {ActualRounds} rounds). Excluded {FinalizedRounds} finalized rounds.",
                studentTrack.Id, sessionId, percentage, attendedCompletedRounds, actualRoundsCount, finalizedRounds.Count);

            var existingAttendanceRecord =
                await attendanceRecordRepository.GetByUserIdAndSessionIdAsync(studentTrack.Id, sessionId,
                    context.CancellationToken);

            var newStatus = percentage >= AttendanceThresholdPercentage
                ? AttendanceStatus.Present
                : AttendanceStatus.Absent;

            if (existingAttendanceRecord is null)
            {
                existingAttendanceRecord =
                    AttendanceRecord.Create(studentTrack.Id, sessionId, newStatus, false, percentage);
                logger.LogInformation(
                    "Creating new AttendanceRecord for Student {StudentId} in Session {SessionId}. Status: {Status}, Percentage: {Percentage:F2}% (based on {ActualRounds} actual rounds)",
                    studentTrack.Id, sessionId, newStatus, percentage, actualRoundsCount);
            }
            else
            {
                if (!Equals(existingAttendanceRecord.Status, newStatus) ||
                    Math.Abs(existingAttendanceRecord.PercentageAttended - percentage) > 0.01)
                {
                    existingAttendanceRecord.Update(newStatus, false, null, percentage);
                    logger.LogInformation(
                        "Updating existing AttendanceRecord for Student {StudentId} in Session {SessionId}. Old Status: {OldStatus}, New Status: {NewStatus}, Old Percentage: {OldPercentage:F2}%, New Percentage: {NewPercentage:F2}% (based on {ActualRounds} actual rounds)",
                        studentTrack.Id, sessionId, existingAttendanceRecord.Status, newStatus,
                        existingAttendanceRecord.PercentageAttended, percentage, actualRoundsCount);
                }
                else
                {
                    logger.LogDebug(
                        "AttendanceRecord for Student {StudentId} in Session {SessionId} remains unchanged.",
                        studentTrack.Id, sessionId);
                }
            }

            attendanceRecordsToProcess.Add(existingAttendanceRecord);
        }

        foreach (var record in attendanceRecordsToProcess)
            await attendanceRecordRepository.AddOrUpdateAsync(record, context.CancellationToken);

        await attendanceRecordRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Finished processing final attendance for Session {SessionId}. Created/Updated {Count} attendance records based on {ActualRounds} actual rounds (excluded {FinalizedRounds} finalized rounds).",
            sessionId, attendanceRecordsToProcess.Count, actualRoundsCount, finalizedRounds.Count);
    }
}
