namespace Zentry.SharedKernel.BLE;

public static class LocationCalculator
{
    public static Point CalculateLocation(IReadOnlyList<(Point BeaconPosition, double Distance)> beaconData)
    {
        if (beaconData.Count < 3) throw new ArgumentException("At least three beacons are required for trilateration.");

        // Simplified trilateration (assuming 2D plane)
        // This is a placeholder; actual implementation may use more complex algorithms
        var x = beaconData.Average(b => b.BeaconPosition.X);
        var y = beaconData.Average(b => b.BeaconPosition.Y);
        return new Point { X = x, Y = y };
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}