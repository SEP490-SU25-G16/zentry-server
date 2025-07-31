using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetStudentDailyClasses;

public record GetStudentDailyClassesQuery(Guid StudentId, DateTime Date)
    : IQuery<List<StudentDailyClassDto>>;