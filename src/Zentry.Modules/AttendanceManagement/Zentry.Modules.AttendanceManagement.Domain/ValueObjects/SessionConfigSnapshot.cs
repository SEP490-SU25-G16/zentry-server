namespace Zentry.Modules.AttendanceManagement.Domain.ValueObjects;

public record SessionConfigSnapshot(
    int AttendanceWindowMinutes,
    int FaceIdVerificationTimeoutSeconds,
    int TotalAttendanceRounds,
    int AbsentReportGracePeriodHours,
    int ManualAdjustmentGracePeriodHours)
{
}
