using Zentry.Modules.ScheduleManagement.Domain.Enums;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreatedScheduleResponse
{
    public Guid Id { get; set; }
    public Guid LecturerId { get; set; }
    public Guid CourseId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public DateTime CreatedAt { get; set; }
}
