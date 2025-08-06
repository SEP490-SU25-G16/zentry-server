namespace Zentry.SharedKernel.Exceptions;

/// <summary>
///     Represents an exception thrown when an account is locked.
/// </summary>
public class AccountLockedException : BusinessLogicException
{
    public AccountLockedException() : base("Account is locked.")
    {
    }

    public AccountLockedException(string message) : base(message)
    {
    }

    public AccountLockedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}