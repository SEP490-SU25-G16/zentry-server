using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ReportingService.Features.ViewClassAttendanceReport;

public record ViewClassAttendanceReportQuery(Guid CourseId, DateTime? StartDate, DateTime? EndDate)
    : IQuery<AttendanceReportResponse>;