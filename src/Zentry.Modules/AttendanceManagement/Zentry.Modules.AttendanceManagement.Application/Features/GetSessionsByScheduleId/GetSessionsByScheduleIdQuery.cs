using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetSessionsByScheduleId;

public record GetSessionsByScheduleIdQuery(Guid ScheduleId) : IQuery<List<SessionDto>>;