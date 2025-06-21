namespace Zentry.Modules.Schedule.Application.Dtos;

public class ScheduleDto
{
    public Guid ScheduleId { get; set; }
    public string CourseName { get; set; }
    public string LecturerName { get; set; }
    public string RoomName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}