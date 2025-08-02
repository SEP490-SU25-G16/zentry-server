using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetStudentDailySchedules;

public record GetStudentDailySchedulesQuery(Guid StudentId, DateTime Date)
    : IQuery<List<StudentDailyClassDto>>;
