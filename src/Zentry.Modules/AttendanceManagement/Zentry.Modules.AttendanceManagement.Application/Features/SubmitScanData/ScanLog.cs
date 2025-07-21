using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

public class ScanLog : AggregateRoot<Guid>
{
    private ScanLog() : base(Guid.Empty) { }

    private ScanLog(
        Guid id,
        Guid deviceId, // DeviceId của người gửi scan
        Guid submitterUserId,
        Guid sessionId, // SessionId của scan
        DateTime timestamp,
        List<ScannedDevice> scannedDevices
    ) : base(id)
    {
        DeviceId = deviceId;
        SubmitterUserId = submitterUserId;
        SessionId = sessionId;
        Timestamp = timestamp;
        ScannedDevices = scannedDevices;
    }

    public Guid DeviceId { get; private set; } // DeviceId của thiết bị đã gửi bản scan
    public Guid SubmitterUserId { get; private set; } // User liên kết với DeviceId đó
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
