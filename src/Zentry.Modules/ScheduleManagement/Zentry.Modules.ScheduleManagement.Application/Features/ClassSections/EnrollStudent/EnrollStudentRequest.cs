namespace Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.EnrollStudent;

public class EnrollStudentRequest
{
    public Guid ClassSectionId { get; set; }
    public Guid StudentId { get; set; }
}