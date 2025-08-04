namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public string Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public int DurationInMinutes { get; set; }
}
