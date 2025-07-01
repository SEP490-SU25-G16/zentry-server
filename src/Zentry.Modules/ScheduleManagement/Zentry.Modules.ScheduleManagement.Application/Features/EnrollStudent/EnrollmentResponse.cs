namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollmentResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid ScheduleId { get; set; } // Changed from CourseId to ScheduleId
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } // Để trả về thông tin sinh viên thân thiện
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; }
}