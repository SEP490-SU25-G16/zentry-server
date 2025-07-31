namespace Zentry.SharedKernel.Exceptions;

/// <summary>
///     Represents an exception thrown when a specific setting is not found.
/// </summary>
public class SettingNotFoundException : Exception
{
    public SettingNotFoundException(string settingName) : base($"Setting '{settingName}' not found")
    {
    }

    public SettingNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}