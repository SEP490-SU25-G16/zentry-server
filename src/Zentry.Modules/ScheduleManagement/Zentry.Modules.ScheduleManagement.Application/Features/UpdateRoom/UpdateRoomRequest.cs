namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateRoom;

public record UpdateRoomRequest(
    string RoomName,
    string Building,
    int Capacity
);
