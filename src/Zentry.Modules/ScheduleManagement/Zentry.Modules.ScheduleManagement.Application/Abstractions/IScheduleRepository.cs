using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Enums.Schedule;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IScheduleRepository : IRepository<Schedule, Guid>
{
    Task<bool> IsLecturerAvailableAsync(Guid lecturerId, WeekDayEnum weekDay, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken);

    Task<bool> IsRoomAvailableAsync(Guid roomId, WeekDayEnum weekDay, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken);

    Task<Tuple<List<Schedule>, int>> GetPagedSchedulesAsync(ScheduleListCriteria criteria,
        CancellationToken cancellationToken);

    Task<Tuple<List<Schedule>, int>> GetPagedSchedulesWithIncludesAsync(
        ScheduleListCriteria criteria,
        CancellationToken cancellationToken);

    Task<Schedule?> GetByIdWithClassSectionAsync(Guid id, CancellationToken cancellationToken);

    public Task<List<Schedule>> GetLecturerSchedulesForDateAsync(
        Guid lecturerId,
        DateTime date,
        WeekDayEnum weekDay,
        CancellationToken cancellationToken);

}
