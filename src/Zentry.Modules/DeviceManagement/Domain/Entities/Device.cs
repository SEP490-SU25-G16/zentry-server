using Domain.Enums;
using Domain.ValueObjects;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Domain.Entities;

public class Device : AggregateRoot
{
    private Device() : base(Guid.Empty)
    {
    } // For EF Core

    private Device(Guid deviceId, Guid accountId, DeviceName deviceName, DeviceToken deviceToken)
        : base(deviceId)
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        Guard.AgainstNull(deviceToken, nameof(deviceToken));
        DeviceId = deviceId;
        AccountId = accountId;
        DeviceName = deviceName;
        DeviceToken = deviceToken;
        RegisteredAt = DateTime.UtcNow;
        Status = DeviceStatus.Active;
    }

    public Guid DeviceId { get; private set; }
    public Guid AccountId { get; }
    public DeviceName DeviceName { get; private set; }
    public DeviceToken DeviceToken { get; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? LastVerifiedAt { get; private set; }
    public DeviceStatus Status { get; private set; }

    public static Device Register(Guid accountId, DeviceName deviceName, DeviceToken deviceToken)
    {
        return new Device(Guid.NewGuid(), accountId, deviceName, deviceToken);
    }

    public void Update(DeviceName deviceName, DeviceStatus status, bool isAdmin, Guid requestingUserId)
    {
        if (!isAdmin && AccountId != requestingUserId)
            throw new UnauthorizedAccessException("User can only update their own device.");

        DeviceName = deviceName ?? DeviceName;
        Status = status;
    }

    public void Delete(bool isAdmin, Guid requestingUserId)
    {
        if (!isAdmin && AccountId != requestingUserId)
            throw new UnauthorizedAccessException("User can only delete their own device.");

        // Soft delete or mark as inactive
        Status = DeviceStatus.Inactive;
    }

    public bool VerifyToken(string token)
    {
        if (DeviceToken.Value != token) return false;
        LastVerifiedAt = DateTime.UtcNow;
        return true;
    }
}