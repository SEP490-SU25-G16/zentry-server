using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Room : Entity
{
    private Room() : base(Guid.Empty)
    {
    } // For EF Core

    public Room(Guid roomId, string name, string building, int capacity) : base(roomId)
    {
        RoomId = roomId;
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name cannot be empty.", nameof(name));
        Building = !string.IsNullOrWhiteSpace(building)
            ? building
            : throw new ArgumentException("Building cannot be empty.", nameof(building));
        Capacity = capacity > 0
            ? capacity
            : throw new ArgumentException("Capacity must be positive.", nameof(capacity));
    }

    public Guid RoomId { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Building { get; private set; }
    public int Capacity { get; private set; }

    // Static factory method để tạo Room mới
    public static Room Create(string name, string building, int capacity)
    {
        return new Room(Guid.NewGuid(), name, building, capacity);
    }
}