namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class ScanLog
{
    private ScanLog()
    {
    } // Private constructor for factory method

    public Guid Id { get; private set; }
    public Guid DeviceId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid SessionId { get; private set; }
    public string RequestId { get; private set; }
    public string RssiData { get; set; } = string.Empty;
    public List<string> NearbyDevices { get; private set; } = [];
    public DateTime Timestamp { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ScanLog Create(
        Guid deviceId,
        Guid studentId,
        Guid sessionId,
        string requestId,
        string rssiData,
        List<string> nearbyDevices,
        DateTime timestamp)
    {
        return new ScanLog
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            StudentId = studentId,
            SessionId = sessionId,
            RequestId = requestId,
            RssiData = rssiData,
            NearbyDevices = nearbyDevices ?? [],
            Timestamp = timestamp,
            CreatedAt = DateTime.UtcNow
        };
    }
}
