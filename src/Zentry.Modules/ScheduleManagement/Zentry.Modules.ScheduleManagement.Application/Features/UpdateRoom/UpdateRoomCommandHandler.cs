using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateRoom;

public class UpdateRoomCommandHandler(IRoomRepository roomRepository)
    : ICommandHandler<UpdateRoomCommand, RoomDetailDto>
{
    public async Task<RoomDetailDto> Handle(UpdateRoomCommand command, CancellationToken cancellationToken)
    {
        // 1. Tìm phòng học trong database
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);

        // 2. Kiểm tra nếu không tìm thấy
        if (room == null) throw new Exception($"Room with ID '{command.Id}' not found.");

        // 3. Business Rule: Kiểm tra RoomName mới có duy nhất không (nếu RoomName thay đổi)
        if (room.RoomName != command.RoomName)
        {
            var isRoomNameUnique =
                await roomRepository.IsRoomNameUniqueExcludingSelfAsync(command.Id, command.RoomName,
                    cancellationToken);
            if (!isRoomNameUnique)
                throw new Exception($"Room with name '{command.RoomName}' already exists for another room.");
        }

        // 4. Áp dụng các thay đổi cho Domain Entity
        room.Update(
            command.RoomName,
            command.Building,
            command.Capacity
        );

        // 5. Lưu các thay đổi vào database
        roomRepository.Update(room); // Entity Framework sẽ theo dõi và cập nhật
        await roomRepository.SaveChangesAsync(cancellationToken);

        // 6. Ánh xạ từ Domain Entity đã cập nhật sang DTO để trả về
        var responseDto = new RoomDetailDto
        {
            Id = room.Id,
            RoomName = room.RoomName,
            Building = room.Building,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };

        return responseDto;
    }
}
