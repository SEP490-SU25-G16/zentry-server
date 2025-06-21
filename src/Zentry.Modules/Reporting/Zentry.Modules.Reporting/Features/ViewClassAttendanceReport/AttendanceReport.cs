namespace Zentry.Modules.Reporting.Features.ViewClassAttendanceReport;

public class AttendanceReport
{
    private AttendanceReport()
    {
    } // For EF Core

    public AttendanceReport(Guid courseId, int totalStudents, int totalSessions, decimal averageAttendanceRate)
    {
        CourseId = courseId != Guid.Empty
            ? courseId
            : throw new ArgumentException("CourseId cannot be empty.", nameof(courseId));
        TotalStudents = totalStudents >= 0
            ? totalStudents
            : throw new ArgumentException("TotalStudents cannot be negative.", nameof(totalStudents));
        TotalSessions = totalSessions >= 0
            ? totalSessions
            : throw new ArgumentException("TotalSessions cannot be negative.", nameof(totalSessions));
        AverageAttendanceRate = averageAttendanceRate >= 0 && averageAttendanceRate <= 100
            ? averageAttendanceRate
            : throw new ArgumentException("AverageAttendanceRate must be between 0 and 100.",
                nameof(averageAttendanceRate));
        GeneratedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CourseId { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public int TotalStudents { get; private set; }
    public int TotalSessions { get; private set; }
    public decimal AverageAttendanceRate { get; private set; }
}