using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Rooms.DeleteRoom;

public class DeleteRoomCommandHandler(
    IRoomRepository roomRepository,
    IScheduleRepository scheduleRepository)
    : ICommandHandler<DeleteRoomCommand, bool>
{
    public async Task<bool> Handle(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);

        if (room is null)
            throw new ResourceNotFoundException($"Room with ID '{command.Id}' not found.");

        if (await scheduleRepository.IsBookedScheduleByRoomIdAsync(room.Id, cancellationToken))
            throw new ResourceCannotBeDeletedException($"Room with ID '{command.Id}' can not be deleted.");

        await roomRepository.SoftDeleteAsync(room.Id, cancellationToken);

        return true;
    }
}