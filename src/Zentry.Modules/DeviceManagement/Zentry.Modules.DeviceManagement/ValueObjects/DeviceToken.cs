using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.ValueObjects;

public class DeviceToken : ValueObject
{
    private DeviceToken(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DeviceToken Create()
    {
        // Generate unique token (e.g., base64-encoded GUID)
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-");
        Guard.AgainstNullOrEmpty(token, nameof(token));
        if (token.Length > 255)
            throw new ArgumentException("Device token cannot exceed 255 characters.", nameof(token));
        return new DeviceToken(token);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}