using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;
using Zentry.SharedKernel.Enums.Device;

namespace Zentry.Modules.DeviceManagement.Entities;

public class Device : AggregateRoot<Guid>
{
    // Private constructor for EF Core and internal use
    private Device() : base(Guid.Empty)
    {
    }

    // Domain-driven constructor
    private Device(
        Guid id,
        Guid userId,
        DeviceName deviceName,
        DeviceToken deviceToken,
        string? platform,
        string? osVersion,
        string? model,
        string? manufacturer,
        string? appVersion,
        string? pushNotificationToken
    ) : base(id)
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        Guard.AgainstNull(deviceToken, nameof(deviceToken));

        UserId = userId;
        DeviceName = deviceName;
        DeviceToken = deviceToken;
        Platform = platform;
        OsVersion = osVersion;
        Model = model;
        Manufacturer = manufacturer;
        AppVersion = appVersion;
        PushNotificationToken = pushNotificationToken;

        CreatedAt = DateTime.UtcNow;
        Status = DeviceStatus.Active; // Initial status upon creation
    }

    public Guid UserId { get; private set; }
    public DeviceName DeviceName { get; private set; }
    public DeviceToken DeviceToken { get; }

    // Các trường bổ sung
    public string? Platform { get; private set; } // Ví dụ: "iOS", "Android", "Web"
    public string? OsVersion { get; private set; } // Ví dụ: "17.5.1", "14"
    public string? Model { get; private set; } // Ví dụ: "iPhone15,3", "SM-G998B"
    public string? Manufacturer { get; private set; } // Ví dụ: "Apple Inc.", "Samsung"
    public string? AppVersion { get; private set; } // Phiên bản ứng dụng đang chạy, ví dụ: "1.0.0", "1.2.3"
    public string? PushNotificationToken { get; private set; } // Token cho push notifications (FCM/APNS)

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; private set; } = DateTime.UtcNow;
    public DeviceStatus Status { get; private set; }

    // Factory method to create a new Device
    public static Device Create(
        Guid userId,
        DeviceName deviceName,
        DeviceToken deviceToken,
        string? platform = null,
        string? osVersion = null,
        string? model = null,
        string? manufacturer = null,
        string? appVersion = null,
        string? pushNotificationToken = null
    )
    {
        var device = new Device(
            Guid.NewGuid(),
            userId,
            deviceName,
            deviceToken,
            platform,
            osVersion,
            model,
            manufacturer,
            appVersion,
            pushNotificationToken
        );
        // device.AddDomainEvent(new DeviceRegisteredEvent(device.Id, device.UserId, device.CreatedAt));
        return device;
    }

    // Method for updating the device's state
    public void Update(
        DeviceName deviceName,
        DeviceStatus status,
        string? platform = null,
        string? osVersion = null,
        string? model = null,
        string? manufacturer = null,
        string? appVersion = null,
        string? pushNotificationToken = null
    )
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        DeviceName = deviceName;
        Status = status;

        if (platform != null) Platform = platform;
        if (osVersion != null) OsVersion = osVersion;
        if (model != null) Model = model;
        if (manufacturer != null) Manufacturer = manufacturer;
        if (appVersion != null) AppVersion = appVersion;
        if (pushNotificationToken != null) PushNotificationToken = pushNotificationToken;

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