using System.Globalization;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.Configuration;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class FinalAttendanceConsumer(
    ILogger<FinalAttendanceConsumer> logger,
    IStudentTrackRepository studentTrackRepository,
    IRoundRepository roundRepository,
    IAttendanceRecordRepository attendanceRecordRepository,
    IRedisService redisService,
    IMediator mediator)
    : IConsumer<SessionFinalAttendanceToProcessMessage>
{
    private const double DefaultAttendanceThresholdPercentage = 75.0;
    private const string AttendanceThresholdConfigKey = "AttendanceThresholdPercentage";

    public async Task Consume(ConsumeContext<SessionFinalAttendanceToProcessMessage> context)
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

    private async Task<double> GetAttendanceThresholdPercentageAsync(Guid sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetHierarchicalSettingsIntegrationQuery(
                [AttendanceThresholdConfigKey],
                [
                    new ScopeContext(ScopeType.Global.ToString()),
                    new ScopeContext(ScopeType.Session.ToString(), sessionId)
                ]
            );

            var response = await mediator.Send(query, cancellationToken);

            if (response.SettingsByKey.TryGetValue(AttendanceThresholdConfigKey, out var settingResult) &&
                settingResult.EffectiveSetting != null &&
                double.TryParse(settingResult.EffectiveSetting.Value, out var threshold))
            {
                var effectiveSetting = settingResult.EffectiveSetting;
                logger.LogInformation(
                    "Using attendance threshold from configuration: {Threshold}% from {ScopeType} scope (ID: {SettingId}, ScopeId: {ScopeId}). Total settings found: {TotalSettings}",
                    threshold,
                    effectiveSetting.ScopeType,
                    effectiveSetting.Id,
                    effectiveSetting.ScopeId,
                    settingResult.AllMatchingSettings.Count);

                // Log hierarchy for debugging
                if (settingResult.AllMatchingSettings.Count <= 1) return threshold;
                var hierarchy = settingResult.AllMatchingSettings
                    .OrderBy(s => s.ScopeType == ScopeType.Global.ToString() ? 1 : 0) // Global first, then others
                    .Select(s => $"{s.ScopeType}:{s.ScopeId}={s.Value}")
                    .ToList();

                logger.LogDebug("Setting hierarchy for '{ConfigKey}': [{Hierarchy}]",
                    AttendanceThresholdConfigKey, string.Join(", ", hierarchy));

                return threshold;
            }

            logger.LogWarning(
                "Attendance threshold setting '{ConfigKey}' not found in any scope (Global, Session:{SessionId}). Using default value: {DefaultThreshold}%",
                AttendanceThresholdConfigKey, sessionId, DefaultAttendanceThresholdPercentage);

            return DefaultAttendanceThresholdPercentage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error retrieving attendance threshold from hierarchical configuration. Using default value: {DefaultThreshold}%",
                DefaultAttendanceThresholdPercentage);
            return DefaultAttendanceThresholdPercentage;
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

        // Get attendance threshold from hierarchical configuration (Global -> Session)
        var attendanceThresholdPercentage = await GetAttendanceThresholdPercentageAsync(sessionId, cancellationToken);

        IReadOnlyList<StudentTrack> relevantStudentTracks;
        List<Round> completedRounds;
        List<AttendanceRecord> allAttendanceRecords;

        try
        {
            var allRounds = await roundRepository.GetRoundsBySessionIdAsync(sessionId, cancellationToken);
            completedRounds = allRounds.Where(r => Equals(r.Status, RoundStatus.Completed)).ToList();

            // Get all attendance records for this session (should include Future status records)
            allAttendanceRecords = await attendanceRecordRepository
                .GetAttendanceRecordsBySessionIdAsync(sessionId, cancellationToken);

            // Get student tracks (students who were actually tracked/scanned during session)
            relevantStudentTracks = await studentTrackRepository
                .GetStudentTracksBySessionIdAsync(sessionId, cancellationToken);

            if (allAttendanceRecords == null || allAttendanceRecords.Count == 0)
            {
                logger.LogError(
                    "No AttendanceRecords found for Session {SessionId}. Records should be created when session is created.",
                    sessionId);
                throw new InvalidOperationException($"Attendance records not found for session {sessionId}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching initial data for session {SessionId}. Aborting process.", sessionId);
            throw;
        }

        logger.LogInformation(
            "Session {SessionId} status: {CompletedRounds} completed rounds, {ActualRounds} rounds used for calculation, {TotalAttendanceRecords} total attendance records, {TrackedStudents} tracked students. Using threshold: {Threshold}%",
            sessionId, completedRounds.Count, actualRoundsCount, allAttendanceRecords.Count,
            relevantStudentTracks.Count, attendanceThresholdPercentage);

        var attendanceRecordsToProcess = new List<AttendanceRecord>();

        // Create a dictionary of student tracks for faster lookup
        var studentTrackDict = relevantStudentTracks.ToDictionary(st => st.StudentId, st => st);

        // Process each attendance record
        foreach (var attendanceRecord in allAttendanceRecords)
            try
            {
                AttendanceStatus newStatus;
                double percentage;
                var oldStatus = attendanceRecord.Status;

                // Check if student was tracked (participated in the session)
                if (studentTrackDict.TryGetValue(attendanceRecord.StudentId, out var studentTrack))
                {
                    // Student participated - calculate actual attendance percentage
                    var attendedCompletedRounds = studentTrack.Rounds
                        .Count(rp => completedRounds.Any(r => r.Id == rp.RoundId) && rp.IsAttended);

                    percentage = actualRoundsCount > 0
                        ? (double)attendedCompletedRounds / actualRoundsCount * 100.0
                        : 0.0;
                    newStatus = percentage >= attendanceThresholdPercentage
                        ? AttendanceStatus.Present
                        : AttendanceStatus.Absent;

                    logger.LogDebug(
                        "Calculated attendance for Student {StudentId} in Session {SessionId}: {Percentage:F2}% (Attended {AttendedRounds} / Actual {ActualRounds} rounds) - Threshold: {Threshold}%",
                        attendanceRecord.StudentId, sessionId, percentage, attendedCompletedRounds, actualRoundsCount,
                        attendanceThresholdPercentage);
                }
                else
                {
                    // Student did not participate (was not tracked/scanned) -> Absent
                    newStatus = AttendanceStatus.Absent;
                    percentage = 0.0;

                    logger.LogDebug(
                        "Student {StudentId} in Session {SessionId} was not tracked (did not participate) -> marked as Absent",
                        attendanceRecord.StudentId, sessionId);
                }

                // Update attendance record if there are changes
                if (!Equals(attendanceRecord.Status, newStatus) ||
                    Math.Abs(attendanceRecord.PercentageAttended - percentage) > 0.01)
                {
                    attendanceRecord.Update(newStatus, false, null, percentage);

                    logger.LogInformation(
                        "Updated AttendanceRecord for Student {StudentId} in Session {SessionId}: {OldStatus} -> {NewStatus}, Percentage: {Percentage:F2}% (Threshold: {Threshold}%)",
                        attendanceRecord.StudentId, sessionId, oldStatus, newStatus, percentage,
                        attendanceThresholdPercentage);
                }
                else
                {
                    logger.LogDebug(
                        "AttendanceRecord for Student {StudentId} in Session {SessionId} remains unchanged",
                        attendanceRecord.StudentId, sessionId);
                }

                attendanceRecordsToProcess.Add(attendanceRecord);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing attendance record for student {StudentId} in session {SessionId}. Skipping this student.",
                    attendanceRecord.StudentId, sessionId);
            }

        try
        {
            // Batch save all updated records
            foreach (var record in attendanceRecordsToProcess)
                await attendanceRecordRepository.AddOrUpdateAsync(record, cancellationToken);

            await attendanceRecordRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving attendance records for session {SessionId}. Aborting.", sessionId);
            throw;
        }

        // Log final statistics
        var presentCount = attendanceRecordsToProcess.Count(r => Equals(r.Status, AttendanceStatus.Present));
        var absentCount = attendanceRecordsToProcess.Count(r => Equals(r.Status, AttendanceStatus.Absent));
        var futureCount = attendanceRecordsToProcess.Count(r => Equals(r.Status, AttendanceStatus.Future));

        logger.LogInformation(
            "Finished processing final attendance for Session {SessionId}. Updated {Total} records: {Present} Present, {Absent} Absent, {Future} Future (based on {ActualRounds} actual rounds with {Threshold}% threshold)",
            sessionId, attendanceRecordsToProcess.Count, presentCount, absentCount, futureCount, actualRoundsCount,
            attendanceThresholdPercentage);
    }
}