using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailySchedules;

public record GetLecturerDailySchedulesQuery(Guid LecturerId, DateTime Date)
    : IQuery<List<LecturerDailyClassDto>>;
