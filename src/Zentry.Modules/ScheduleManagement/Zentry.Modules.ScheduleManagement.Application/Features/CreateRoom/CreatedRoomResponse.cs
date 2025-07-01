namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateRoom;

public class CreatedRoomResponse
{
    public Guid Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
}