namespace Zentry.SharedKernel.Exceptions;

public class SelectionDataTypeRequiresOptionsException()
    : BusinessLogicException("Attribute Definition with DataType 'Selection' must have options provided.");