using Microsoft.EntityFrameworkCore;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Enums.Attendance;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class SessionRepository(AttendanceDbContext dbContext) : ISessionRepository
{
    public async Task<Session?> GetSessionByScheduleIdAndDate(Guid scheduleId, DateTime date,
        CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .Where(s => s.ScheduleId == scheduleId && s.StartTime.Date == date.Date)
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
