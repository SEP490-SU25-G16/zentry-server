namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class RoundAttendanceDto
{
    public Guid RoundId { get; set; }
    public Guid SessionId { get; set; }

    public int RoundNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int AttendedCount { get; set; }
    public int TotalStudents { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid? CourseId { get; set; }
    public string? CourseCode { get; set; }
    public string? CourseName { get; set; }

    public Guid? ClassSectionId { get; set; }
    public string? SectionCode { get; set; }
}