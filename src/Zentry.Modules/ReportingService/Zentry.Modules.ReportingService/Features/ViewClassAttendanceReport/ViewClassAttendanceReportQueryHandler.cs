using MediatR;
using Zentry.Modules.ReportingService.Persistence;

namespace Zentry.Modules.ReportingService.Features.ViewClassAttendanceReport;

public class
    ViewClassAttendanceReportQueryHandler(ReportingDbContext reportingDbContext)
    : IRequestHandler<ViewClassAttendanceReportQuery, AttendanceReportResponse>
{
    private readonly ReportingDbContext _reportingDbContext = reportingDbContext;

    public Task<AttendanceReportResponse> Handle(ViewClassAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        // var query = from enrollment in _attendanceDbContext.Set<AttendanceReport>()
        //             join attendanceRecord in _attendanceDbContext.Set<AttendanceReport>()
        //                 on enrollment.Id equals attendanceRecord.EnrollmentId
        //             join round in _attendanceDbContext.Set<AttendanceReport>()
        //                 on attendanceRecord.RoundId equals round.Id
        //             where enrollment.CourseId == request.CourseId
        //             select new { enrollment, attendanceRecord, round };
        //
        // if (request.StartDate.HasValue && request.EndDate.HasValue)
        // {
        //     query = query.Where(x => x.round.StartTime >= request.StartDate.Value && x.round.EndTime <= request.EndDate.Value);
        // }
        //
        // var result = await query
        //     .GroupBy(x => x.enrollment.CourseId)
        //     .Select(g => new
        //     {
        //         CourseId = g.Key,
        //         TotalStudents = g.Select(x => x.enrollment.StudentId).Distinct().Count(),
        //         TotalSessions = g.Select(x => x.round.Id).Distinct().Count(),
        //         AttendedSessions = g.Sum(x => x.attendanceRecord.IsPresent ? 1 : 0)
        //     })
        //     .FirstOrDefaultAsync(cancellationToken);
        //
        // if (result == null)
        // {
        //     return new AttendanceReportDto
        //     {
        //         CourseId = request.CourseId,
        //         GeneratedAt = DateTime.UtcNow,
        //         TotalStudents = 0,
        //         TotalSessions = 0,
        //         AverageAttendanceRate = 0
        //     };
        // }
        //
        // var averageAttendanceRate = result.TotalSessions > 0
        //     ? (decimal)result.AttendedSessions / result.TotalSessions * 100
        //     : 0;
        //
        // return new AttendanceReportDto
        // {
        //     CourseId = result.CourseId,
        //     GeneratedAt = DateTime.UtcNow,
        //     TotalStudents = result.TotalStudents,
        //     TotalSessions = result.TotalSessions,
        //     AverageAttendanceRate = Math.Round(averageAttendanceRate, 2)
        // };
        throw new NotImplementedException("not implemented");
    }
}
