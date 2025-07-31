namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class MonthlyCalendarResponseDto
{
    public List<DailyScheduleDto> CalendarDays { get; set; } = new();
}

public class DailyScheduleDto
{
    public DateTime Date { get; set; }
    public List<CalendarClassDto> Classes { get; set; } = [];
}

// DTO cho mỗi lớp học trong một ngày
public class CalendarClassDto
{
    public TimeOnly StartTime { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public Guid ClassSectionId { get; set; }
}