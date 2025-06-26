using Zentry.Modules.DeviceManagement.Domain.Enums;
using Zentry.Modules.DeviceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.Domain.Entities;
public class Device : AggregateRoot<Guid>
{
    private Device() : base(Guid.Empty) { }
    private Device(Guid id, Guid userId, DeviceName deviceName, DeviceToken deviceToken)
        : base(id)
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        Guard.AgainstNull(deviceToken, nameof(deviceToken));
        UserId = userId;
        DeviceName = deviceName;
        DeviceToken = deviceToken;
        CreatedAt = DateTime.UtcNow;
        Status = DeviceStatus.Active;
    }
    public Guid UserId { get; }
    public DeviceName DeviceName { get; private set; }
    public DeviceToken DeviceToken { get; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; private set; }
    public DeviceStatus Status { get; private set; }

    public static Device Create(Guid userId, DeviceName deviceName, DeviceToken deviceToken)
    {
        return new Device(Guid.NewGuid(), userId, deviceName, deviceToken);
    }

    public void Update(DeviceName deviceName, DeviceStatus status)
    {
        DeviceName = deviceName ?? DeviceName;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = DeviceStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool VerifyToken(string token)
    {
        if (DeviceToken.Value != token) return false;
        LastVerifiedAt = DateTime.UtcNow;
        return true;
    }
}
