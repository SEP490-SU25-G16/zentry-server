namespace Zentry.SharedKernel.Exceptions;

public class ResourceCannotBeDeletedException : BusinessLogicException
{
    public ResourceCannotBeDeletedException(Guid id) : base(
        $"Resource with ID '{id}' can not be deleted.")
    {
    }

    public ResourceCannotBeDeletedException(string message) : base(message)
    {
    }

    public ResourceCannotBeDeletedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
