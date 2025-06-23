using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class ScheduleRepository(ScheduleDbContext context) : IScheduleRepository
{
    public Task<Domain.Entities.Schedule> GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Domain.Entities.Schedule>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Domain.Entities.Schedule>> FindAsync(ISpecification<Domain.Entities.Schedule> specification,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Domain.Entities.Schedule entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Domain.Entities.Schedule entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Domain.Entities.Schedule entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasConflictAsync(Guid roomId, Guid lecturerId, DateTime startTime, DateTime endTime,
        Guid? excludeScheduleId = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<Domain.Entities.Schedule>> GetSchedulesByCourseIdsAsync(List<Guid> courseIds, DateTime startDate,
        DateTime endDate)
    {
        throw new NotImplementedException();
    }
}