namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class LecturerDailyReportDto
{
    public string LecturerName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string RoomInfo { get; set; } = string.Empty;
    public string TimeSlot { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int AttendedStudents { get; set; }
    public int PresentStudents { get; set; }
    public int LateStudents { get; set; }
    public int AbsentStudents { get; set; }
    public string AttendanceRate { get; set; } = string.Empty;
    public string OnTimeRate { get; set; } = string.Empty;
}
