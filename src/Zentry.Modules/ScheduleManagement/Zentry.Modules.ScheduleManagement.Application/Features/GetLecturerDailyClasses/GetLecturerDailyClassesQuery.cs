using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyClasses;

public record GetLecturerDailyClassesQuery(Guid LecturerId, DateTime Date)
    : IQuery<List<LecturerDailyClassDto>>;
