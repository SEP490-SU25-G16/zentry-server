namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class LecturerHomeDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public int EnrolledStudents { get; set; }
    public int TotalSessions { get; set; }
    public List<ScheduleInfoDto> Schedules { get; set; } = [];
}

public class ScheduleInfoDto
{
    public string RoomInfo { get; set; } = string.Empty;
    public string ScheduleInfo { get; set; } = string.Empty;
}
