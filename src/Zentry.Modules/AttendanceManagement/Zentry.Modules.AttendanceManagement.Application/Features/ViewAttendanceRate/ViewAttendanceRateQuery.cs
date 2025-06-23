using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.ViewAttendanceRate;

public record ViewAttendanceRateQuery(Guid StudentId, Guid CourseId)
    : IQuery<AttendanceRateDto>, IRequest<AttendanceRateDto>;