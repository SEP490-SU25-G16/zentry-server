namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class LecturerDailyClassDto
{
    public Guid ScheduleId { get; set; }
    public Guid ClassSectionId { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public string SectionCode { get; set; }
    public string RoomName { get; set; }
    public string Building { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public int TotalSessions { get; set; } // Giả sử đã tính được
    public string SessionProgress { get; set; } // Ví dụ: "Buổi 5/10"
    public string SessionStatus { get; set; } // Trạng thái của Session (Pending, Active, Completed, v.v.)
    public bool CanStartSession { get; set; } // Logic để bật/tắt nút "Start Session"
    public string Weekday { get; set; } // Ví dụ: "Thứ Hai", "Tuesday"
    public DateOnly DateInfo { get; set; } // Ngày cụ thể của lịch trình này
    public string LecturerName { get; set; } // Tên của giảng viên
}