using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetRooms;

public class GetRoomsQuery : IQuery<GetRoomsResponse>
{
    public GetRoomsQuery()
    {
        // Parameterless constructor cho model binding từ query string
    }

    public GetRoomsQuery(int pageNumber, int pageSize, string? searchTerm = null, string? building = null,
        string? sortBy = null, string? sortOrder = null)
    {
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
        PageSize = pageSize <= 0 ? 10 : pageSize;
        SearchTerm = searchTerm?.Trim();
        Building = building?.Trim(); // Lọc theo tòa nhà
        SortBy = sortBy?.Trim();
        SortOrder = sortOrder?.Trim();
    }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? SearchTerm { get; init; }
    public string? Building { get; init; }
    public string? SortBy { get; init; } = "CreatedAt"; // Mặc định sắp xếp theo ngày tạo
    public string? SortOrder { get; init; } = "desc"; // Mặc định giảm dần
}