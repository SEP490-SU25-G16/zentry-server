namespace Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;

public class ClassSectionListCriteria
{
    public Guid? CourseId { get; set; }
    public Guid? LecturerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
}
