using MediatR;
using Zentry.Modules.Attendance.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.Attendance.Application.Features.ViewAttendanceRate;

public record ViewAttendanceRateQuery(Guid StudentId, Guid CourseId) : IQuery<AttendanceRateDto>, IRequest<AttendanceRateDto>;
