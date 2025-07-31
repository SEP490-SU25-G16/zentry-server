using Marten;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class MartenSessionWhitelistRepository(IDocumentSession session) : IScanLogWhitelistRepository
{
    public async Task AddAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default)
    {
        session.Store(whitelist);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<SessionWhitelist?> GetBySessionIdAsync(Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await session.Query<SessionWhitelist>()
            .FirstOrDefaultAsync(w => w.SessionId == sessionId, cancellationToken);
    }

    public async Task UpdateAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default)
    {
        session.Store(whitelist);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await session.Query<SessionWhitelist>()
            .AnyAsync(cancellationToken);
    }

    public async Task<List<SessionWhitelist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (List<SessionWhitelist>)await session.Query<SessionWhitelist>()
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<SessionWhitelist> whitelists,
        CancellationToken cancellationToken = default)
    {
        session.StoreObjects(whitelists);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        session.DeleteWhere<SessionWhitelist>(w => true);
        await SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await session.SaveChangesAsync(cancellationToken);
    }
}