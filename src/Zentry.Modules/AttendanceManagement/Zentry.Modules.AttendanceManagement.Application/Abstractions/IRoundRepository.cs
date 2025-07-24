using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IRoundRepository : IRepository<Round, Guid>
{
    Task<List<Round>> GetRoundsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
