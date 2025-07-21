using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetCourses;

public class GetCoursesQueryHandler(ICourseRepository courseRepository)
    : IQueryHandler<GetCoursesQuery, GetCoursesResponse>
{
    public async Task<GetCoursesResponse> Handle(GetCoursesQuery query, CancellationToken cancellationToken)
    {
        // Ánh xạ Query sang Criteria để truyền xuống Repository
        var criteria = new CourseListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            Semester = query.Semester,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        };

        // Lấy dữ liệu đã phân trang từ Repository
        var (courses, totalCount) = await courseRepository.GetPagedCoursesAsync(criteria, cancellationToken);

        // Ánh xạ từ Domain Entities sang DTOs
        var courseDtos = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt
        }).ToList();

        // Tạo đối tượng phản hồi
        var response = new GetCoursesResponse
        {
            Items = courseDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return response;
    }
}
