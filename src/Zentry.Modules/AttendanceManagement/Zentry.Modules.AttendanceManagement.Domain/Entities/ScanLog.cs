namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class ScanLog
{
    private ScanLog()
    {
    } // Private constructor for factory method

    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid RoundId { get; set; }
    public string RssiData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ScanLog Create(Guid deviceId, Guid roundId, string rssiData, DateTime timestamp)
    {
        return new ScanLog
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            RoundId = roundId,
            RssiData = rssiData,
            Timestamp = timestamp,
            CreatedAt = DateTime.UtcNow
        };
    }
}
