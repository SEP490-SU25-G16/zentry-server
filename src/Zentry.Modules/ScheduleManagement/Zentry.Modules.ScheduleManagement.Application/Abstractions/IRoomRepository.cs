using Zentry.Modules.ScheduleManagement.Application.Features.GetRooms;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IRoomRepository : IRepository<Room, Guid>
{
    Task<bool> IsRoomNameUniqueAsync(string roomName, CancellationToken cancellationToken);

    Task<Tuple<List<Room>, int>> GetPagedRoomsAsync(RoomListCriteria criteria, CancellationToken cancellationToken);

    // Thêm phương thức này để kiểm tra RoomName có duy nhất không, ngoại trừ bản thân phòng đó
    Task<bool> IsRoomNameUniqueExcludingSelfAsync(Guid roomId, string roomName, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Room>> GetByRoomNamesAsync(List<string> roomNames, CancellationToken cancellationToken);
}
