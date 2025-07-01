namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class EnrollmentListItemDto
{
    public Guid EnrollmentId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public Guid StudentId { get; set; }
    public string? StudentCode { get; set; }
    public string? StudentName { get; set; }
    public Guid ScheduleId { get; set; }
    public string? ScheduleName { get; set; } // Sẽ được lấy từ tên của Course trong Schedule
    public Guid CourseId { get; set; }
    public string? CourseCode { get; set; } // Mới
    public string? CourseName { get; set; }
    public Guid RoomId { get; set; } // Cập nhật, từ Schedule
    public string? RoomName { get; set; } // Cập nhật, từ Room lookup
    public Guid LecturerId { get; set; } // Cập nhật, từ Schedule
    public string? LecturerName { get; set; } // Cập nhật, từ Lecturer lookup
    public DateTime StartTime { get; set; } // Cập nhật, từ Schedule
    public DateTime EndTime { get; set; } // Cập nhật, từ Schedule
    public string? DayOfWeek { get; set; } // Cập nhật, từ Schedule.DayOfWeek.ToString()
    public string? Status { get; set; }
}