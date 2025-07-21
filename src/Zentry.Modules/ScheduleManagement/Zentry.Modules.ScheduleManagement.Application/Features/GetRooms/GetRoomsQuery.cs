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

public class GetRoomsResponse
{
    public List<RoomListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}