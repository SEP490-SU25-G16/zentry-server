namespace Zentry.SharedKernel.Exceptions;

/// <summary>
/// Represents a base class for all business logic related exceptions.
/// </summary>
public abstract class BusinessLogicException : Exception
{
    public BusinessLogicException(string message) : base(message)
    {
    }

    public BusinessLogicException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
