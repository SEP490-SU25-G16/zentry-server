using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteRoom;

public class DeleteRoomCommandHandler(IRoomRepository roomRepository) : ICommandHandler<DeleteRoomCommand, bool>
{
    public async Task<bool> Handle(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        // 1. Tìm phòng học trong database
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);

        // 2. Kiểm tra nếu không tìm thấy
        if (room == null)
            // Ném ngoại lệ nếu không tìm thấy phòng học
            throw new Exception($"Room with ID '{command.Id}' not found.");

        // 3. Thực hiện xóa cứng (hard delete)
        roomRepository.Delete(room); // Gọi phương thức Delete trên repository
        await roomRepository.SaveChangesAsync(cancellationToken);

        return true; // Trả về true để xác nhận xóa thành công
    }
}