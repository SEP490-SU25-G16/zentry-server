using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteRoom;

public class DeleteRoomCommandHandler(IRoomRepository roomRepository) : ICommandHandler<DeleteRoomCommand, bool>
{
    public async Task<bool> Handle(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);

        if (room == null)
            throw new Exception($"Room with ID '{command.Id}' not found.");

        await roomRepository.DeleteAsync(room, cancellationToken);

        return true; // Trả về true để xác nhận xóa thành công
    }
}
