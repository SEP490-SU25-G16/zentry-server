using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class SessionRepository(AttendanceDbContext context) : ISessionRepository
{
    public async Task AddAsync(Session entity, CancellationToken cancellationToken)
    {
        await context.Sessions.AddAsync(entity, cancellationToken);
    }

    public Task<IEnumerable<Session>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Session entity, CancellationToken cancellationToken)
    {
        context.Sessions.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Session entity, CancellationToken cancellationToken)
    {
        context.Sessions.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
