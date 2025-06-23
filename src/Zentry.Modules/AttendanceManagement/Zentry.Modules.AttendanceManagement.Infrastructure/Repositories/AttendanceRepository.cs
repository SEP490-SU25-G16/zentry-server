using Microsoft.EntityFrameworkCore;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class AttendanceRepository(AttendanceDbContext context) : IAttendanceRepository
{
    public async Task<(int TotalSessions, int AttendedSessions)> GetAttendanceStatsAsync(Guid studentId, Guid courseId)
    {
        var stats = await context.AttendanceRecords
            .Join(context.Enrollments,
                ar => ar.EnrollmentId,
                e => e.Id,
                (ar, e) => new { ar, e })
            .Where(x => x.e.StudentId == studentId && x.e.CourseId == courseId)
            .GroupBy(x => new { x.e.StudentId, x.e.CourseId })
            .Select(g => new
            {
                TotalSessions = g.Count(),
                AttendedSessions = g.Sum(x => x.ar.IsPresent ? 1 : 0)
            })
            .FirstOrDefaultAsync();

        return stats != null ? (stats.TotalSessions, stats.AttendedSessions) : (0, 0);
    }

    public Task<AttendanceRecord> GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AttendanceRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AttendanceRecord>> FindAsync(ISpecification<AttendanceRecord> specification,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(AttendanceRecord attendanceRecord)
    {
        await context.AttendanceRecords.AddAsync(attendanceRecord);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}