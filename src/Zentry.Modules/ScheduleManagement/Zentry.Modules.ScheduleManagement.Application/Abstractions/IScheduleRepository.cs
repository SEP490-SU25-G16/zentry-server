using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IScheduleRepository : IRepository<Schedule, Guid>
{
    Task<bool> IsLecturerAvailableAsync(Guid lecturerId, DayOfWeekEnum dayOfWeek, DateTime startTime, DateTime endTime,
        CancellationToken cancellationToken);

    Task<bool> IsRoomAvailableAsync(Guid roomId, DayOfWeekEnum dayOfWeek, DateTime startTime, DateTime endTime,
        CancellationToken cancellationToken);

    Task<Tuple<List<Schedule>, int>> GetPagedSchedulesAsync(ScheduleListCriteria criteria,
        CancellationToken cancellationToken);

    Task<Tuple<List<Schedule>, int>> GetPagedSchedulesWithIncludesAsync(
        ScheduleListCriteria criteria,
        CancellationToken cancellationToken);

    Task<Schedule?> GetByIdWithClassSectionAsync(Guid id, CancellationToken cancellationToken);
}
