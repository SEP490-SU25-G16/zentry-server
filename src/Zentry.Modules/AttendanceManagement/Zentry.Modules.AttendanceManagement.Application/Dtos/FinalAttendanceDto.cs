namespace Zentry.Modules.AttendanceManagement.Application.Dtos;

public class FinalAttendanceDto
{
    public Guid StudentId { get; set; }
    public string? StudentFullName { get; set; }
    public string? Email { get; set; } // Mới: email sinh viên
    public string? PhoneNumber { get; set; } // Mới: số điện thoại sinh viên
    public string? Status { get; set; } // Trạng thái điểm danh (Present, Late, Absent)

    // Thông tin đăng ký (Enrollment)
    public DateTime EnrolledAt { get; set; } // Mới: ngày đăng ký
    public string? EnrollmentStatus { get; set; } // Mới: trạng thái đăng ký (Active, Inactive, etc.)

    // Thông tin chi tiết buổi học/lớp học
    public string? ClassInfo { get; set; } // Mới: lop_hoc (kết hợp mã môn và mã lớp, ví dụ: "CS101 - SE1701")
    public DateTime SessionStartTime { get; set; } // Mới: thời gian bắt đầu session

    // Trạng thái điểm danh chi tiết
    public string? DetailedAttendanceStatus { get; set; } // Mới: trang_thai_tham_gia (chuỗi mô tả tiếng Việt)
    public DateTime? LastAttendanceTime { get; set; } // Mới: thời điểm điểm danh của bản ghi cuối cùng
}
