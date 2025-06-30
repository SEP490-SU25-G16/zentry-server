using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetCourses;

public class GetCoursesQuery : IQuery<GetCoursesResponse>
{
    public GetCoursesQuery()
    {
        // Parameterless constructor cho model binding từ query string
    }

    public GetCoursesQuery(int pageNumber, int pageSize, string? searchTerm = null, string? semester = null,
        string? sortBy = null, string? sortOrder = null)
    {
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
        PageSize = pageSize <= 0 ? 10 : pageSize;
        SearchTerm = searchTerm?.Trim();
        Semester = semester?.Trim();
        SortBy = sortBy?.Trim();
        SortOrder = sortOrder?.Trim();
    }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? SearchTerm { get; init; }
    public string? Semester { get; init; } // Lọc theo học kỳ
    public string? SortBy { get; init; } = "CreatedAt"; // Mặc định sắp xếp theo ngày tạo
    public string? SortOrder { get; init; } = "desc"; // Mặc định giảm dần
}