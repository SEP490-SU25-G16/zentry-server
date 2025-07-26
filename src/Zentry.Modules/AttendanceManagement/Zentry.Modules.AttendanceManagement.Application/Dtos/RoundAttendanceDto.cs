namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class RoundAttendanceDto
{
    public int RoundNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int AttendedCount { get; set; }
    public int TotalStudents { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CourseCode { get; set; }
    public string? SectionCode { get; set; }
}
