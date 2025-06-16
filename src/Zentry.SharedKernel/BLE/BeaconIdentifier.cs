namespace Zentry.SharedKernel.BLE;

public class BeaconIdentifier
{
    public BeaconIdentifier(Guid beaconId, string deviceName)
    {
        BeaconId = beaconId;
        DeviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
    }

    public Guid BeaconId { get; }
    public string DeviceName { get; }

    public override bool Equals(object obj)
    {
        if (obj is BeaconIdentifier other) return BeaconId == other.BeaconId && DeviceName == other.DeviceName;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BeaconId, DeviceName);
    }
}