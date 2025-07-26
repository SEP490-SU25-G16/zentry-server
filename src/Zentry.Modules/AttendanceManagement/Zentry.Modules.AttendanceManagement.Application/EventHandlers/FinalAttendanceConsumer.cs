using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Enums.Attendance;

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
        logger.LogInformation(
            "Received SessionFinalAttendanceToProcess for Session {SessionId} at {Timestamp}. Starting final attendance calculation.",
            sessionId, context.Message.Timestamp);

        var totalRoundsInSession =
            await roundRepository.CountRoundsBySessionIdAsync(sessionId, context.CancellationToken);
        if (totalRoundsInSession == 0)
        {
            logger.LogWarning("No rounds found for Session {SessionId}. Skipping final attendance calculation.",
                sessionId);
            return;
        }

        // Đảm bảo kiểu trả về là IReadOnlyList<StudentTrack> nếu bạn đã thay đổi trong interface
        IReadOnlyList<StudentTrack> relevantStudentTracks =
            await studentTrackRepository.GetStudentTracksBySessionIdAsync(sessionId,
                context.CancellationToken);

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
            var percentage = studentTrack.CalculatePercentageAttended(totalRoundsInSession);

            logger.LogDebug(
                "Calculated attendance for Student {StudentId} in Session {SessionId}: {Percentage:F2}% (Attended {AttendedRounds} / Total {TotalRounds} rounds).",
                studentTrack.Id, sessionId, percentage, studentTrack.Rounds.Count(rp => rp.IsAttended),
                totalRoundsInSession);

            var existingAttendanceRecord =
                await attendanceRecordRepository.GetByUserIdAndSessionIdAsync(studentTrack.Id, sessionId,
                    context.CancellationToken);

            AttendanceStatus newStatus = percentage >= AttendanceThresholdPercentage
                ? AttendanceStatus.Present
                : AttendanceStatus.Absent;

            if (existingAttendanceRecord is null)
            {
                existingAttendanceRecord =
                    AttendanceRecord.Create(studentTrack.Id, sessionId, newStatus, false, percentage);
                logger.LogInformation(
                    "Creating new AttendanceRecord for Student {StudentId} in Session {SessionId}. Status: {Status}, Percentage: {Percentage:F2}%",
                    studentTrack.Id, sessionId, newStatus, percentage);
            }
            else
            {
                if (existingAttendanceRecord.Status != newStatus ||
                    Math.Abs(existingAttendanceRecord.PercentageAttended - percentage) > 0.01)
                {
                    existingAttendanceRecord.Update(newStatus, false, null, percentage);
                    logger.LogInformation(
                        "Updating existing AttendanceRecord for Student {StudentId} in Session {SessionId}. Old Status: {OldStatus}, New Status: {NewStatus}, Old Percentage: {OldPercentage:F2}%, New Percentage: {NewPercentage:F2}%",
                        studentTrack.Id, sessionId, existingAttendanceRecord.Status, newStatus,
                        existingAttendanceRecord.PercentageAttended, percentage);
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

        // Sử dụng AddOrUpdateAsync cho từng bản ghi
        foreach (var record in attendanceRecordsToProcess)
        {
            await attendanceRecordRepository.AddOrUpdateAsync(record, context.CancellationToken);
        }

        // Chỉ gọi SaveChangesAsync một lần sau khi tất cả các thao tác Add/Update đã được thực hiện
        await attendanceRecordRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Finished processing final attendance for Session {SessionId}. Created/Updated {Count} attendance records.",
            sessionId, attendanceRecordsToProcess.Count);
    }
}
