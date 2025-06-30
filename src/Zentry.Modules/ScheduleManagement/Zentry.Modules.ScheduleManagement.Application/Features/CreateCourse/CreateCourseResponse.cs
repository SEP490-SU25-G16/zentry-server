namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateCourse;

public class CourseCreatedResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
}
