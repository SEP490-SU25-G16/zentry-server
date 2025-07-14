namespace Zentry.SharedKernel.Contracts;

public class ScheduleLookupDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid LecturerId { get; set; } // ID của giảng viên được phân công cho buổi học này
    public DateTime ScheduledStartTime { get; set; } // Thời gian bắt đầu buổi học
    public DateTime ScheduledEndTime { get; set; }   // Thời gian kết thúc buổi học
    public bool IsActive { get; set; } // Trạng thái của lịch trình có hợp lệ không
    // Thêm các thông tin khác nếu cần cho việc kiểm tra
}
