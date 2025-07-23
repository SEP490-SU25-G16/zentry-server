using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IScanLogWhitelistRepository
{
    Task AddAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default);

    Task<SessionWhitelist?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task UpdateAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default);
}