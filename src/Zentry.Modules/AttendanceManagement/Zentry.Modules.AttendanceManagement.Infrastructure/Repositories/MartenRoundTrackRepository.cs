using Marten;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class MartenRoundTrackRepository(IDocumentSession documentSession) : IRoundTrackRepository
{
    public async Task AddOrUpdateAsync(RoundTrack roundTrack, CancellationToken cancellationToken)
    {
        documentSession.Store(roundTrack); // Marten tự động biết để thêm mới hoặc cập nhật dựa vào Id
        await documentSession.SaveChangesAsync(cancellationToken);
    }

    public async Task<RoundTrack?> GetByIdAsync(Guid roundId, CancellationToken cancellationToken)
    {
        return await documentSession.LoadAsync<RoundTrack>(roundId, cancellationToken);
    }
}