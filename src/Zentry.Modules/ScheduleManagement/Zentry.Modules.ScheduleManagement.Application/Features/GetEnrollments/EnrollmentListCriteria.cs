using Zentry.Modules.ScheduleManagement.Domain.Enums;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;

public class EnrollmentListCriteria
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ScheduleId { get; set; }
    public Guid? CourseId { get; set; }
    public EnrollmentStatus? Status { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
