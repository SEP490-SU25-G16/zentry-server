using Zentry.Modules.ScheduleManagement.Domain.Enums;

namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid LecturerId { get; set; }
    public string LecturerName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public Guid ClassSectionId { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
