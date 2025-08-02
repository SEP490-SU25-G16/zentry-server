namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class StudentAttendanceDto
{
    public Guid StudentId { get; set; }
    public string? FullName { get; set; }
    public bool IsAttended { get; set; }
    public DateTime? AttendedTime { get; set; }
}
