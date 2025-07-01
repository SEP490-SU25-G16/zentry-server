using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateRoom;

public class UpdateRoomCommandHandler(IRoomRepository roomRepository)
    : ICommandHandler<UpdateRoomCommand, RoomDetailDto>
{
    public async Task<RoomDetailDto> Handle(UpdateRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);

        if (room == null) throw new Exception($"Room with ID '{command.Id}' not found.");

        if (room.RoomName != command.RoomName)
        {
            var isRoomNameUnique =
                await roomRepository.IsRoomNameUniqueExcludingSelfAsync(command.Id, command.RoomName,
                    cancellationToken);
            if (!isRoomNameUnique)
                throw new Exception($"Room with name '{command.RoomName}' already exists for another room.");
        }

        room.Update(
            command.RoomName,
            command.Building,
            command.Capacity
        );

        await roomRepository.UpdateAsync(room, cancellationToken);

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
