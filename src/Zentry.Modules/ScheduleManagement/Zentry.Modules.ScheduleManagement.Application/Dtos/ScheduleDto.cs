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
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public WeekDayEnum WeekDay { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
