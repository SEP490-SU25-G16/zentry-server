using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<List<Session>> GetSessionsByScheduleIdsAndDatesAsync(
        List<Guid> scheduleIds,
        List<DateOnly> localDates,
        CancellationToken cancellationToken);

    Task<List<Session>> GetSessionsByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken);

    Task<Session?> GetSessionByScheduleIdAndDate(Guid scheduleId, DateTime date,
        CancellationToken cancellationToken);

    Task<Guid> GetLecturerIdBySessionId(Guid sessionId, CancellationToken cancellationToken);

    Task<Session?> GetSessionByScheduleIdAndDateAsync(Guid scheduleId, DateOnly date,
        CancellationToken cancellationToken);

    Task<List<Session>> GetSessionsByScheduleIdsAndDateAsync(
        List<Guid> scheduleIds, DateOnly date, CancellationToken cancellationToken);

    Task<DateTime?> GetActualEndTimeAsync(Guid sessionId, CancellationToken cancellationToken);
}