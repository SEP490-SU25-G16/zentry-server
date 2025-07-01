using Zentry.Modules.ScheduleManagement.Domain.Enums;

namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid LecturerId { get; set; }
    public string LecturerName { get; set; } = string.Empty; // Tên giảng viên
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty; // Tên khóa học
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty; // Tên phòng
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}