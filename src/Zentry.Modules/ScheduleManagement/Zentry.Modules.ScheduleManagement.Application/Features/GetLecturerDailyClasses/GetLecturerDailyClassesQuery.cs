using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Dtos;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyClasses;

public record GetLecturerDailyClassesQuery(Guid LecturerId, DateTime Date)
    : IRequest<List<LecturerDailyClassDto>>;
