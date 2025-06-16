namespace Zentry.SharedKernel.BLE;

public class RSSI
{
    public RSSI(int value, DateTime timestamp)
    {
        Value = value;
        Timestamp = timestamp;
    }

    public int Value { get; } // RSSI value in dBm
    public DateTime Timestamp { get; }
}