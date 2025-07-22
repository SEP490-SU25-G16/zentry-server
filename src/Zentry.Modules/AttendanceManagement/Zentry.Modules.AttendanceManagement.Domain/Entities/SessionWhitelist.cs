namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class SessionWhitelist
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public List<string> WhitelistedDeviceIds { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    public SessionWhitelist()
    {
        WhitelistedDeviceIds = new List<string>();
    }

    public SessionWhitelist(Guid id, Guid sessionId, List<string> whitelistedDeviceIds)
    {
        Id = id;
        SessionId = sessionId;
        WhitelistedDeviceIds = whitelistedDeviceIds ?? new List<string>();
        GeneratedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public static SessionWhitelist Create(Guid sessionId, List<string> whitelistedDeviceIds)
    {
        return new SessionWhitelist(Guid.NewGuid(), sessionId, whitelistedDeviceIds);
    }

    public void UpdateWhitelist(List<string> newWhitelist)
    {
        WhitelistedDeviceIds = newWhitelist ?? new List<string>();
        LastUpdatedAt = DateTime.UtcNow;
    }
}
