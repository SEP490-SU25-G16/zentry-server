namespace Zentry.Modules.ReportingService.Features.ViewClassAttendanceReport;

public class AttendanceReportResponse
{
    public Guid CourseId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int TotalStudents { get; set; }
    public int TotalSessions { get; set; }
    public decimal AverageAttendanceRate { get; set; }
}