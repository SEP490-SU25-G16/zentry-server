namespace Zentry.SharedKernel.Models;

/// <summary>
/// Model chứa thông tin session
/// </summary>
public class SessionData
{
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public SessionData(Guid userId, Guid deviceId, DateTime createdAt, DateTime expiresAt)
    {
        UserId = userId;
        DeviceId = deviceId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }
}
