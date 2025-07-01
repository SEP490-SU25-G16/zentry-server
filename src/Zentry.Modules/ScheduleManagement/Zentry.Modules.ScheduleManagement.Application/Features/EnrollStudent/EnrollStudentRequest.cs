namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentRequest
{
    public Guid ScheduleId { get; set; } // Changed from CourseId
    public Guid StudentId { get; set; }
}