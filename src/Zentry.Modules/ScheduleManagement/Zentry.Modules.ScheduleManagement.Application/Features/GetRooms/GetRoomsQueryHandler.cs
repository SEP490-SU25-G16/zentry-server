using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetRooms;

public class GetRoomsQueryHandler(IRoomRepository roomRepository) : IQueryHandler<GetRoomsQuery, GetRoomsResponse>
{
    public async Task<GetRoomsResponse> Handle(GetRoomsQuery query, CancellationToken cancellationToken)
    {
        // Ánh xạ Query sang Criteria để truyền xuống Repository
        var criteria = new RoomListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            Building = query.Building,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        };

        // Lấy dữ liệu đã phân trang từ Repository
        var (rooms, totalCount) = await roomRepository.GetPagedRoomsAsync(criteria, cancellationToken);

        // Ánh xạ từ Domain Entities sang DTOs
        var roomDtos = rooms.Select(r => new RoomListItemDto
        {
            Id = r.Id,
            RoomName = r.RoomName,
            Building = r.Building,
            Capacity = r.Capacity,
            CreatedAt = r.CreatedAt
        }).ToList();

        // Tạo đối tượng phản hồi
        var response = new GetRoomsResponse
        {
            Items = roomDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return response;
    }
}