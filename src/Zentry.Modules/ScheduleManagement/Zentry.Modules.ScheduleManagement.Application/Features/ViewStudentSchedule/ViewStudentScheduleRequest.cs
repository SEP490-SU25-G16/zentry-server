using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Dtos;

namespace Zentry.Modules.ScheduleManagement.Application.Features.ViewStudentSchedule;

public record ViewStudentScheduleRequest(DateTime StartDate, DateTime EndDate) : IRequest<List<ScheduleDto>>;