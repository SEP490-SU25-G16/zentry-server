namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;

public class CreateClassSectionRequest
{
    public string CourseId { get; init; }
    public string SectionCode { get; init; } = default!;
    public string Semester { get; init; } = default!;
}
