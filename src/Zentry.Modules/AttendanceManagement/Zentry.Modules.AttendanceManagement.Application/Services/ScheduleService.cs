using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    public Task<ScheduleLookupDto?> GetScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsLecturerAssignedToScheduleAsync(Guid lecturerId, Guid scheduleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
