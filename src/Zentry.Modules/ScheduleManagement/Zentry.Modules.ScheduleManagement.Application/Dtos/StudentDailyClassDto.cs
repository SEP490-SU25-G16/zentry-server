namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class StudentDailyClassDto
{
    public Guid ScheduleId { get; set; }
    public Guid ClassSectionId { get; set; }

    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;

    public string SectionCode { get; set; } = string.Empty;

    public Guid LecturerId { get; set; } // Giảng viên của lớp học
    public string LecturerName { get; set; } = string.Empty;

    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Weekday { get; set; }
    public DateOnly DateInfo { get; set; }

    // Thông tin về buổi học (sessions) của lớp này
    public List<SessionInfoDto> Sessions { get; set; } = [];

    // Có thể thêm các trường liên quan đến điểm danh, trạng thái điểm danh của sinh viên
    public Guid StudentId { get; set; } // StudentId của sinh viên đang query
    // Ví dụ: public string StudentAttendanceStatus { get; set; } // Trạng thái điểm danh của sinh viên cho buổi học này
}
