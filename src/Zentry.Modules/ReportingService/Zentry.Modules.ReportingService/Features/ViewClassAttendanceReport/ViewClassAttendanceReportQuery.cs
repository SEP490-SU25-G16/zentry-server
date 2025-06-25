using MediatR;

namespace Zentry.Modules.ReportingService.Features.ViewClassAttendanceReport;

public record ViewClassAttendanceReportQuery(Guid CourseId, DateTime? StartDate, DateTime? EndDate)
    : IRequest<AttendanceReportResponse>;