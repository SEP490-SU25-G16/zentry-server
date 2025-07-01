using Zentry.Modules.DeviceManagement.Domain.Enums;
using Zentry.Modules.DeviceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.Domain.Entities;

public class Device : AggregateRoot<Guid>
{
    // Private constructor for EF Core and internal use
    private Device() : base(Guid.Empty)
    {
    }

    // Domain-driven constructor
    private Device(Guid id, Guid userId, DeviceName deviceName, DeviceToken deviceToken)
        : base(id)
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        Guard.AgainstNull(deviceToken, nameof(deviceToken));
        UserId = userId;
        DeviceName = deviceName;
        DeviceToken = deviceToken;
        CreatedAt = DateTime.UtcNow;
        Status = DeviceStatus.Active; // Initial status upon creation
    }

    public Guid UserId { get; private set; } // Set private set as it's set via constructor/factory
    public DeviceName DeviceName { get; private set; }
    public DeviceToken DeviceToken { get; } // Private set as it's generated once
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; private set; }
    public DeviceStatus Status { get; private set; }

    // Factory method to create a new Device
    public static Device Create(Guid userId, DeviceName deviceName, DeviceToken deviceToken)
    {
        // Consider adding domain events here if needed, e.g., DeviceRegisteredEvent
        var device = new Device(Guid.NewGuid(), userId, deviceName, deviceToken);
        // device.AddDomainEvent(new DeviceRegisteredEvent(device.Id, device.UserId, device.CreatedAt));
        return device;
    }

    // Methods for updating the device's state
    public void Update(DeviceName deviceName, DeviceStatus status)
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        DeviceName = deviceName; // Assuming new deviceName replaces old
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
        Guard.AgainstNullOrEmpty(token, nameof(token));
        if (DeviceToken.Value != token) return false;

        LastVerifiedAt = DateTime.UtcNow;
        return true;
    }
}