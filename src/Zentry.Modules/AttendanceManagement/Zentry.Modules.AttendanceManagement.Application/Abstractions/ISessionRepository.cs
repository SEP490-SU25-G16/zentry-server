using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<IEnumerable<Session>> GetSessionsByScheduleIdAndStatusAsync(Guid scheduleId, SessionStatus status,
        CancellationToken cancellationToken);

    Task<Session?> GetActiveSessionByScheduleId(Guid scheduleId, CancellationToken cancellationToken);
    Task<List<Session>> GetSessionsByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken);

    Task<Session?> GetSessionByScheduleIdAndDate(Guid scheduleId, DateTime date,
        CancellationToken cancellationToken);

    Task<Guid> GetLecturerIdBySessionId(Guid sessionId, CancellationToken cancellationToken);

    Task<Session?> GetSessionByScheduleIdAndDateAsync(Guid scheduleId, DateOnly date,
        CancellationToken cancellationToken);

    Task<List<Session>> GetSessionsByScheduleIdsAndDatesAsync(
        List<Guid> scheduleIds,
        List<DateTime> utcDates,
        CancellationToken cancellationToken);
}