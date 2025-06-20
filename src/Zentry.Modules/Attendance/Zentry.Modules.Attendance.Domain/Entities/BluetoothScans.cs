using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.Attendance.Domain.Entities;

public class BluetoothScan : AggregateRoot
{
    public Guid DeviceId { get; private set; }
    public string BeaconId { get; private set; }
    public int Rssi { get; private set; }
    public DateTime Timestamp { get; private set; }

    private BluetoothScan() : base(Guid.Empty){ } // For MongoDB

    public BluetoothScan(Guid deviceId, string beaconId, int rssi, DateTime timestamp) : base(deviceId)
    {
        DeviceId = deviceId != Guid.Empty ? deviceId : throw new ArgumentException("DeviceId cannot be empty.", nameof(deviceId));
        BeaconId = !string.IsNullOrWhiteSpace(beaconId) ? beaconId : throw new ArgumentException("BeaconId cannot be empty.", nameof(beaconId));
        Rssi = rssi;
        Timestamp = timestamp > DateTime.MinValue ? timestamp : throw new ArgumentException("Timestamp must be valid.", nameof(timestamp));
    }
}
