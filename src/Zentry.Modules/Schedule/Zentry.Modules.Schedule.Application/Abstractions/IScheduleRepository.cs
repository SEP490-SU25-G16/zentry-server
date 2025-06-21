using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.Schedule.Application.Abstractions;

public interface IScheduleRepository : IRepository<Domain.Entities.Schedule>
{
    Task<bool> HasConflictAsync(Guid roomId, Guid lecturerId, DateTime startTime, DateTime endTime,
        Guid? excludeScheduleId = null);

    Task<List<Domain.Entities.Schedule>> GetSchedulesByCourseIdsAsync(List<Guid> courseIds, DateTime startDate,
        DateTime endDate);
}