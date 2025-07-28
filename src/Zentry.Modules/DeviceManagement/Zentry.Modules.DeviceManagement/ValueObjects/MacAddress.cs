using System.Text.RegularExpressions;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.ValueObjects;

public partial class MacAddress : ValueObject
{
    private static readonly Regex MacAddressRegex = MyRegex();

    public string Value { get; }

    private MacAddress(string value)
    {
        Value = NormalizeFormat(value);
    }

    public static MacAddress Create(string macAddress)
    {
        Guard.AgainstNullOrEmpty(macAddress, nameof(macAddress));

        if (!IsValidMacAddress(macAddress))
        {
            throw new ArgumentException($"Invalid MAC address format: {macAddress}", nameof(macAddress));
        }

        return new MacAddress(macAddress);
    }

    private static bool IsValidMacAddress(string macAddress)
    {
        return MacAddressRegex.IsMatch(macAddress);
    }

    private static string NormalizeFormat(string macAddress)
    {
        // Loại bỏ tất cả ký tự không phải hex
        var cleaned = Regex.Replace(macAddress.ToUpperInvariant(), @"[^0-9A-F]", "");

        // Đảm bảo có đúng 12 ký tự
        if (cleaned.Length != 12)
            throw new ArgumentException($"MAC address must be 12 hex characters, got {cleaned.Length}");

        // Format thành AA:BB:CC:DD:EE:FF
        return string.Join(":",
            Enumerable.Range(0, 6)
                .Select(i => cleaned.Substring(i * 2, 2)));
    }

    public string ToRawFormat()
    {
        return Value.Replace(":", "");
    }

    public string ToColonFormat()
    {
        return Value;
    }

    public string ToDashFormat()
    {
        return Value.Replace(":", "-");
    }

    public string ToDotFormat()
    {
        var raw = ToRawFormat();
        return $"{raw.Substring(0, 4)}.{raw.Substring(4, 4)}.{raw.Substring(8, 4)}";
    }

    public bool IsLocallyAdministered()
    {
        var firstByte = Convert.ToByte(Value.Substring(0, 2), 16);
        return (firstByte & 0x02) != 0;
    }

    public bool IsUniversallyAdministered()
    {
        return !IsLocallyAdministered();
    }

    public bool IsMulticast()
    {
        var firstByte = Convert.ToByte(Value.Substring(0, 2), 16);
        return (firstByte & 0x01) != 0;
    }

    public bool IsUnicast()
    {
        return !IsMulticast();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    // ĐÂY LÀ PHẦN QUAN TRỌNG NHẤT GIÚP EF CORE DỊCH ĐƯỢC MACADDRESS
    public static implicit operator string(MacAddress macAddress)
    {
        return macAddress.Value;
    }

    [GeneratedRegex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$|^([0-9A-Fa-f]{12})$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
