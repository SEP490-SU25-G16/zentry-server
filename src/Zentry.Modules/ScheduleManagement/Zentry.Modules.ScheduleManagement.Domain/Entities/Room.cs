using System.ComponentModel.DataAnnotations;
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
        IsDeleted = false;
    }

    [Required] [StringLength(100)] public string RoomName { get; private set; }

    [Required] [StringLength(100)] public string Building { get; private set; }

    [Range(0, 1000)] public int Capacity { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; }


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

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}