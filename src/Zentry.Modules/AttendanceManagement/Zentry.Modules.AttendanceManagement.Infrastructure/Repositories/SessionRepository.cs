using Microsoft.EntityFrameworkCore;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class SessionRepository(AttendanceDbContext dbContext) : ISessionRepository
{
    public async Task<DateTime?> GetActualEndTimeAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => s.ActualEndTime)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<List<Session>> GetSessionsByScheduleIdsAndDateAsync(
        List<Guid> scheduleIds,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var utcDateStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await dbContext.Sessions
            .AsNoTracking()
            .Where(s => scheduleIds.Contains(s.ScheduleId) &&
                        s.StartTime.Date == utcDateStart.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Session>> GetSessionsByScheduleIdsAndDatesAsync(
        List<Guid> scheduleIds,
        List<DateTime> utcDates,
        CancellationToken cancellationToken)
    {
        var distinctScheduleIds = scheduleIds.Distinct().ToList();
        var distinctUtcDates = utcDates.Distinct().ToList();

        return await dbContext.Sessions
            .Where(s => distinctScheduleIds.Contains(s.ScheduleId) &&
                        distinctUtcDates.Contains(s.StartTime
                            .Date))
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetSessionByScheduleIdAndDateAsync(Guid scheduleId, DateOnly date,
        CancellationToken cancellationToken)
    {
        var utcDateStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await dbContext.Sessions
            .Where(s => s.ScheduleId == scheduleId &&
                        s.StartTime.Date == utcDateStart.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Session?> GetSessionByScheduleIdAndDate(Guid scheduleId, DateTime date,
        CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .Where(s => s.ScheduleId == scheduleId && s.StartTime.Date == date.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> GetLecturerIdBySessionId(Guid sessionId, CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .Where(s => s.Id == sessionId)
            .Select(s => s.UserId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Session>> GetSessionsByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .Where(s => s.ScheduleId == scheduleId)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Session entity, CancellationToken cancellationToken)
    {
        await dbContext.Sessions.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Session> entities, CancellationToken cancellationToken)
    {
        await dbContext.Sessions.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<IEnumerable<Session>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Sessions.ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task UpdateAsync(Session entity, CancellationToken cancellationToken)
    {
        dbContext.Sessions.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Session entity, CancellationToken cancellationToken)
    {
        dbContext.Sessions.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Session>> GetSessionsByScheduleIdAndStatusAsync(Guid scheduleId, SessionStatus status,
        CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .Where(s => s.ScheduleId == scheduleId && s.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetActiveSessionByScheduleId(Guid scheduleId, CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.Status == SessionStatus.Active,
                cancellationToken);
    }
}
