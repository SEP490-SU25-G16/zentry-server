namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class ScanLog
{
    private ScanLog(
        Guid id,
        Guid deviceId, // DeviceId của người gửi scan
        Guid submitterUserId,
        Guid sessionId, // SessionId của scan
        DateTime timestamp,
        List<ScannedDevice> scannedDevices
    )
    {
        Id = id;
        DeviceId = deviceId;
        SubmitterUserId = submitterUserId;
        SessionId = sessionId;
        Timestamp = timestamp;
        ScannedDevices = scannedDevices;
    }

    public Guid Id { get; private set; }
    public Guid DeviceId { get; private set; }
    public Guid SubmitterUserId { get; private set; }
    public Guid SessionId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public List<ScannedDevice> ScannedDevices { get; private set; }

    public static ScanLog Create(
        Guid id,
        Guid deviceId,
        Guid submitterUserId,
        Guid sessionId,
        DateTime timestamp,
        List<ScannedDevice> scannedDevices)
    {
        return new ScanLog(id, deviceId, submitterUserId, sessionId, timestamp, scannedDevices);
    }
}
