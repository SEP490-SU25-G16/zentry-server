using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Room : AggregateRoot<Guid>
{
    private Room() : base(Guid.Empty)
    {
    }

    private Room(Guid id, string roomName, string building, int capacity)
        : base(id)
    {
        RoomName = roomName;
        Building = building;
        Capacity = capacity;
        CreatedAt = DateTime.UtcNow;
    }

    public string RoomName { get; private set; }
    public string Building { get; private set; }
    public int Capacity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Room Create(string roomName, string building, int capacity)
    {
        return new Room(Guid.NewGuid(), roomName, building, capacity);
    }

    public void Update(string? roomName = null, string? building = null, int? capacity = null)
    {
        if (!string.IsNullOrWhiteSpace(roomName)) RoomName = roomName;
        if (!string.IsNullOrWhiteSpace(building)) Building = building;
        if (capacity.HasValue) Capacity = capacity.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}
