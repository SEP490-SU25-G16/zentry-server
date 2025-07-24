namespace Zentry.Modules.AttendanceManagement.Application.Dtos;


public class FinalAttendanceDto
{
    public Guid StudentId { get; set; }
    public string? StudentFullName { get; set; }
    public string? Status { get; set; }
}
