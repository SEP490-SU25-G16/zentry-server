using MediatR;

namespace Zentry.Modules.Reporting.Features.ViewClassAttendanceReport;

public record ViewClassAttendanceReportQuery(Guid CourseId, DateTime? StartDate, DateTime? EndDate) : IRequest<AttendanceReportDto>;
