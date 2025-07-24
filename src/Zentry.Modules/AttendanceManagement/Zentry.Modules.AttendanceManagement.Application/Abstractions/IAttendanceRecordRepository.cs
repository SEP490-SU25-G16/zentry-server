using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IAttendanceRecordRepository : IRepository<AttendanceRecord, Guid>
{
    Task<(int TotalSessions, int AttendedSessions)> GetAttendanceStatsAsync(Guid studentId, Guid courseId);
    public Task<List<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken);
}
