namespace Zentry.SharedKernel.BLE;

public static class DistanceEstimator
{
    private const double PathLossExponent = 2.0; // Typical value for indoor environments
    private const int ReferenceRssi = -59; // RSSI at 1 meter (calibrated)

    public static double EstimateDistance(int rssi)
    {
        if (rssi >= ReferenceRssi) return 1.0; // Less than or equal to 1 meter

        var ratio = (double)rssi / ReferenceRssi;
        return Math.Pow(10, (ReferenceRssi - rssi) / (10 * PathLossExponent));
    }
}