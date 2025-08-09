namespace Zentry.Modules.ScheduleManagement.Application.Features.Rooms.CreateRoom;

public class CreateRoomRequest
{
    public required string RoomName { get; set; }
    public required string Building { get; set; }
    public int Capacity { get; set; }
}