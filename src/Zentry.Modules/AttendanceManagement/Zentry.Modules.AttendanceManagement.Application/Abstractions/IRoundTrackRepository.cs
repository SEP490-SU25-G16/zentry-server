using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IRoundTrackRepository
{
    Task AddOrUpdateAsync(RoundTrack roundTrack, CancellationToken cancellationToken);
    Task<RoundTrack?> GetByIdAsync(Guid roundId, CancellationToken cancellationToken);
}
