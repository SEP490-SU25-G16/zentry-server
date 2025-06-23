namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class AttendanceRateDto
{
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public double AttendanceRate { get; set; }
    public string AbsenceStatus { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
}