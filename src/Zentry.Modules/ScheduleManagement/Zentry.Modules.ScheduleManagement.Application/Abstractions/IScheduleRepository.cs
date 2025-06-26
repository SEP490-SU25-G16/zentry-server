using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IScheduleRepository : IRepository<Schedule, Guid>
{
    Task<bool> HasConflictAsync(Guid roomId, Guid lecturerId, DateTime startTime, DateTime endTime,
        Guid? excludeScheduleId = null);

    Task<List<Domain.Entities.Schedule>> GetSchedulesByCourseIdsAsync(List<Guid> courseIds, DateTime startDate,
        DateTime endDate);
}
