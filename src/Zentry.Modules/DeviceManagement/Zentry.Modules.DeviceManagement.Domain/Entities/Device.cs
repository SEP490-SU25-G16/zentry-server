using Zentry.Modules.DeviceManagement.Domain.Enums;
using Zentry.Modules.DeviceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.Domain.Entities;

// Kế thừa từ AggregateRoot<Guid> và loại bỏ thuộc tính DeviceId trùng lặp
public class Device : AggregateRoot<Guid>
{
    // Constructor mặc định cho EF Core
    private Device() : base(Guid.Empty)
    {
    }

    // Constructor chính, sử dụng 'id' được truyền vào làm khóa chính của thực thể
    private Device(Guid deviceId, Guid accountId, DeviceName deviceName, DeviceToken deviceToken)
        : base(deviceId) // Truyền deviceId vào constructor của lớp cơ sở AggregateRoot
    {
        Guard.AgainstNull(deviceName, nameof(deviceName));
        Guard.AgainstNull(deviceToken, nameof(deviceToken));
        // Id đã được gán bởi lớp cơ sở Entity thông qua AggregateRoot
        AccountId = accountId;
        DeviceName = deviceName;
        DeviceToken = deviceToken;
        CreatedAt = DateTime.UtcNow;
        Status = DeviceStatus.Active;
    }

    // Thuộc tính Id đã được kế thừa từ Entity<Guid>
    // public Guid DeviceId { get; private set; } // Loại bỏ thuộc tính này
    public Guid AccountId { get; }
    public DeviceName DeviceName { get; private set; }
    public DeviceToken DeviceToken { get; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; private set; }
    public DeviceStatus Status { get; private set; }

    public static Device Register(Guid accountId, DeviceName deviceName, DeviceToken deviceToken)
    {
        // Sử dụng Guid.NewGuid() để tạo Id cho thực thể mới
        return new Device(Guid.NewGuid(), accountId, deviceName, deviceToken);
    }

    public void Update(DeviceName deviceName, DeviceStatus status, bool isAdmin, Guid requestingUserId)
    {
        // So sánh AccountId với requestingUserId
        if (!isAdmin && AccountId != requestingUserId)
            throw new UnauthorizedAccessException("User can only update their own device.");

        DeviceName = deviceName ?? DeviceName;
        Status = status;
    }

    public void Delete(bool isAdmin, Guid requestingUserId)
    {
        // So sánh AccountId với requestingUserId
        if (!isAdmin && AccountId != requestingUserId)
            throw new UnauthorizedAccessException("User can only delete their own device.");

        // Xóa mềm hoặc đánh dấu không hoạt động
        Status = DeviceStatus.Inactive;
    }

    public bool VerifyToken(string token)
    {
        if (DeviceToken.Value != token) return false;
        LastVerifiedAt = DateTime.UtcNow;
        return true;
    }
}
