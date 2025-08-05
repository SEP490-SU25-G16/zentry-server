namespace Zentry.Modules.AttendanceManagement.Application.Services.Interface;

public interface IAttendanceCalculationService
{
    Task<AttendanceCalculationResult> CalculateAttendanceForRound(
        Guid sessionId,
        Guid roundId,
        CancellationToken cancellationToken);
}