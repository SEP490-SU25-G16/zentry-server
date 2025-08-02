using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface ISessionWhitelistRepository
{
    Task AddAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default);

    Task<SessionWhitelist?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task UpdateAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default);

    // Thêm methods cho seeding
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<List<SessionWhitelist>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<SessionWhitelist> whitelists, CancellationToken cancellationToken = default);

    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}
