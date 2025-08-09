using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetLecturerHome;

public record GetLecturerHomeQuery(Guid LecturerId) : IQuery<List<LecturerHomeDto>>;
