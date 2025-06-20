using Zentry.Modules.Attendance.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.Attendance.Application.Abstractions;

public interface IAttendanceRepository : IRepository<AttendanceRecord>
{
    Task<(int TotalSessions, int AttendedSessions)> GetAttendanceStatsAsync(Guid studentId, Guid courseId);
}
