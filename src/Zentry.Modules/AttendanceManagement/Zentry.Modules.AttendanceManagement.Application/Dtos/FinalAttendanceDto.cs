namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class FinalAttendanceDto
{
    public Guid StudentId { get; set; }
    public string? StudentFullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Status { get; set; }

    public Guid EnrollmentId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string? EnrollmentStatus { get; set; }

    public Guid SessionId { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid CourseId { get; set; }
    public string? ClassInfo { get; set; } // lop_hoc (kết hợp mã môn và mã lớp, ví dụ: "CS101 - SE1701")
    public DateTime SessionStartTime { get; set; }

    public Guid? LastAttendanceRecordId { get; set; }
    public string? DetailedAttendanceStatus { get; set; }
}