namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollMultipleStudents;

public class BulkEnrollStudentsRequest
{
    public Guid ClassSectionId { get; set; }
    public List<Guid> StudentIds { get; set; } = [];
}