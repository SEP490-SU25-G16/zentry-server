using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class ScheduleRepository(ScheduleDbContext context) : IScheduleRepository
{
    public Task<IEnumerable<Schedule>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Schedule entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Update(Schedule entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(Schedule entity)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasConflictAsync(Guid roomId, Guid lecturerId, DateTime startTime, DateTime endTime,
        Guid? excludeScheduleId = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<Schedule>> GetSchedulesByCourseIdsAsync(List<Guid> courseIds, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }
}