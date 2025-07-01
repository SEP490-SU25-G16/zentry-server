using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateRoom;

public class CreateRoomCommandHandler(IRoomRepository roomRepository)
    : ICommandHandler<CreateRoomCommand, RoomCreatedResponseDto>
{
    public async Task<RoomCreatedResponseDto> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        // 1. Business Rule: RoomName phải là duy nhất
        var isRoomNameUnique = await roomRepository.IsRoomNameUniqueAsync(command.RoomName, cancellationToken);
        if (!isRoomNameUnique) throw new Exception($"Room with name '{command.RoomName}' already exists.");

        // 2. Tạo đối tượng Room Domain Entity
        var room = Room.Create(
            command.RoomName,
            command.Building,
            command.Capacity
        );

        // 3. Lưu vào cơ sở dữ liệu thông qua Repository
        await roomRepository.AddAsync(room, cancellationToken);
        await roomRepository.SaveChangesAsync(cancellationToken); // Lưu thay đổi vào DB

        // 4. Ánh xạ từ Domain Entity sang DTO để trả về
        var responseDto = new RoomCreatedResponseDto
        {
            Id = room.Id,
            RoomName = room.RoomName,
            Building = room.Building,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt
        };

        return responseDto;
    }
}