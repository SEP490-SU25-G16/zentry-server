namespace Zentry.SharedKernel.Exceptions;

public class NotFoundException(string name, object key) : Exception($"Entity \"{name}\" ({key}) was not found.");