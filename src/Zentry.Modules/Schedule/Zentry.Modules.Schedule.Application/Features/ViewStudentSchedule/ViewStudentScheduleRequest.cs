using MediatR;
using Zentry.Modules.Schedule.Application.Dtos;

namespace Zentry.Modules.Schedule.Application.Features.ViewStudentSchedule;

public record ViewStudentScheduleRequest(DateTime StartDate, DateTime EndDate) : IRequest<List<ScheduleDto>>;
