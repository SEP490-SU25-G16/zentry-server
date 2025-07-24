using Microsoft.EntityFrameworkCore;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class AttendanceRecordRepository(AttendanceDbContext dbContext) : IAttendanceRecordRepository
{
    public async Task<List<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await dbContext.AttendanceRecords
            .Where(ar => ar.SessionId == sessionId)
            .ToListAsync(cancellationToken);
    }
    public Task<IEnumerable<AttendanceRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AddRangeAsync(IEnumerable<AttendanceRecord> entities, CancellationToken cancellationToken)
    {
        await dbContext.AttendanceRecords.AddRangeAsync(entities, cancellationToken);
    }

    public Task UpdateAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<(int TotalSessions, int AttendedSessions)> GetAttendanceStatsAsync(Guid studentId, Guid courseId)
    {
        throw new NotImplementedException();
    }
}
