using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Constants.Device;
using Zentry.SharedKernel.Domain;

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
        MacAddress macAddress, // Thêm MAC address
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
        Guard.AgainstNull(macAddress, nameof(macAddress));

        UserId = userId;
        DeviceName = deviceName;
        DeviceToken = deviceToken;
        MacAddress = macAddress; // Gán MAC address
        Platform = platform;
        OsVersion = osVersion;
        Model = model;
        Manufacturer = manufacturer;
        AppVersion = appVersion;
        PushNotificationToken = pushNotificationToken;

        CreatedAt = DateTime.UtcNow;
        Status = DeviceStatus.Active;
    }

    public Guid UserId { get; private set; }
    public DeviceName DeviceName { get; private set; }
    public DeviceToken DeviceToken { get; }
    public MacAddress MacAddress { get; private set; } // Thêm property MAC address

    // Các trường bổ sung
    public string? Platform { get; private set; }
    public string? OsVersion { get; private set; }
    public string? Model { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? AppVersion { get; private set; }
    public string? PushNotificationToken { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; private set; } = DateTime.UtcNow;
    public DeviceStatus Status { get; private set; }

    // Factory method to create a new Device
    public static Device Create(
        Guid userId,
        DeviceName deviceName,
        DeviceToken deviceToken,
        MacAddress macAddress, // Thêm MAC address parameter
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
            macAddress,
            platform,
            osVersion,
            model,
            manufacturer,
            appVersion,
            pushNotificationToken
        );
        return device;
    }

    public void Update(
        DeviceName deviceName,
        DeviceStatus status,
        MacAddress? macAddress = null,
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

        if (macAddress != null) MacAddress = macAddress;
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

    // Thêm method để verify MAC address
    public bool VerifyMacAddress(string macAddress)
    {
        Guard.AgainstNullOrEmpty(macAddress, nameof(macAddress));
        return MacAddress.Value.Equals(macAddress, StringComparison.OrdinalIgnoreCase);
    }

    // Method để tìm device theo MAC address
    public static bool IsSameMacAddress(Device device, string macAddress)
    {
        return device.MacAddress.Value.Equals(macAddress, StringComparison.OrdinalIgnoreCase);
    }

    public void UpdateStatus(DeviceStatus newStatus)
    {
        if (Equals(Status, newStatus)) return;

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}