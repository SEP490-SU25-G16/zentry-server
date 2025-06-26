using Microsoft.EntityFrameworkCore;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class AttendanceRepository(AttendanceDbContext context) : IAttendanceRepository
{
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

    public void Update(AttendanceRecord entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(AttendanceRecord entity)
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
