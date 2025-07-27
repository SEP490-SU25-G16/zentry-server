namespace Zentry.SharedKernel.Exceptions;

public class InvalidAttributeDefinitionTypeException(string message) : BusinessLogicException(message);

public class AttributeDefinitionKeyAlreadyExistsException(string key)
    : BusinessLogicException($"Attribute Definition with Key '{key}' already exists.");

public class InvalidSettingValueException(string message) : BusinessLogicException(message);

public class SelectionDataTypeRequiresOptionsException()
    : BusinessLogicException("Attribute Definition with DataType 'Selection' must have options provided.");

public class SettingAlreadyExistsException(string attributeKey, string scopeType, Guid scopeId)
    : BusinessLogicException(
        $"Setting for Attribute '{attributeKey}' with Scope '{scopeType}' and ScopeId '{scopeId}' already exists.");
