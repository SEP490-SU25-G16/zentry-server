using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetRoomById;

public class GetRoomByIdQueryHandler(IRoomRepository roomRepository) : IQueryHandler<GetRoomByIdQuery, RoomDetailDto?>
{
    public async Task<RoomDetailDto?> Handle(GetRoomByIdQuery query, CancellationToken cancellationToken)
    {
        // 1. Lấy Room Entity từ Repository
        var room = await roomRepository.GetByIdAsync(query.Id, cancellationToken);

        // 2. Kiểm tra nếu không tìm thấy
        if (room == null)
            // Ném một NotFoundException để middleware xử lý thành 404
            throw new Exception($"Room with ID '{query.Id}' not found.");
        // Hoặc đơn giản là trả về null và Controller sẽ xử lý thành NotFound()
        // return null;
        // 3. Ánh xạ từ Domain Entity sang DTO để trả về
        var roomDetailDto = new RoomDetailDto
        {
            Id = room.Id,
            RoomName = room.RoomName,
            Building = room.Building,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };

        return roomDetailDto;
    }
}