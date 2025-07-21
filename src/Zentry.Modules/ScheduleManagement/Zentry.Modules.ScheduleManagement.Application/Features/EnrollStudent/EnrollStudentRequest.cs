namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentRequest
{
    public Guid ClassSectionId { get; set; }
    public Guid StudentId { get; set; }
}