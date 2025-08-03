using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetStudentDailySchedules;

public record GetStudentDailySchedulesQuery(Guid StudentId, DateOnly Date)
    : IQuery<List<StudentDailyClassDto>>;
