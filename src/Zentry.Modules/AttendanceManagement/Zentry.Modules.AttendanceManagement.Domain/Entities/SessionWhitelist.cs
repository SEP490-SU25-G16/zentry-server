namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class SessionWhitelist
{
    public SessionWhitelist()
    {
        WhitelistedDeviceIds = new List<Guid>();
    }

    public SessionWhitelist(Guid id, Guid sessionId, List<Guid> whitelistedDeviceIds)
    {
        Id = id;
        SessionId = sessionId;
        WhitelistedDeviceIds = whitelistedDeviceIds;
        GeneratedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public List<Guid> WhitelistedDeviceIds { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    public static SessionWhitelist Create(Guid sessionId, List<Guid> whitelistedDeviceIds)
    {
        return new SessionWhitelist(Guid.NewGuid(), sessionId, whitelistedDeviceIds);
    }

    public void UpdateWhitelist(List<Guid> newWhitelist)
    {
        WhitelistedDeviceIds = newWhitelist;
        LastUpdatedAt = DateTime.UtcNow;
    }
}