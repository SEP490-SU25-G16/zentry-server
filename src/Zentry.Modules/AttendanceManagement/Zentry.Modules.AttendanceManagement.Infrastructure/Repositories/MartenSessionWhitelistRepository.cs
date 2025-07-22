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
        // MartenDB có khả năng truy vấn theo bất kỳ thuộc tính nào của document
        return await session.Query<SessionWhitelist>()
            .FirstOrDefaultAsync(w => w.SessionId == sessionId, cancellationToken);
    }

    public async Task UpdateAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default)
    {
        session.Store(whitelist);
        await SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await session.SaveChangesAsync(cancellationToken);
    }
}
