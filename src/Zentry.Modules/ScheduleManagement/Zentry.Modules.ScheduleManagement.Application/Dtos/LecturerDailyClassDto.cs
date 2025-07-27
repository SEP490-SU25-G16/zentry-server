namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class LecturerDailyClassDto
{
    public Guid ScheduleId { get; set; }
    public Guid ClassSectionId { get; set; }

    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;

    public string SectionCode { get; set; } = string.Empty;

    public Guid RoomId { get; set; } // <-- Thêm RoomId
    public string RoomName { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public int TotalSessions { get; set; }
    public string SessionProgress { get; set; }
    public string SessionStatus { get; set; }
    public bool CanStartSession { get; set; }
    public string Weekday { get; set; }
    public DateOnly DateInfo { get; set; }

    public Guid LecturerId { get; set; }
    public string LecturerName { get; set; } = string.Empty;

    public List<SessionInfoDto> Sessions { get; set; } = [];
}

public class SessionInfoDto
{
    public Guid SessionId { get; set; }
    public Guid ScheduleId { get; set; }
    public int SessionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
