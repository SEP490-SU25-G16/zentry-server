using Zentry.Modules.ConfigurationManagement.Domain.Enums;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Domain.Entities;

public class AttributeDefinition : AggregateRoot<Guid>
{
    private AttributeDefinition() : base(Guid.Empty)
    {
    }

    private AttributeDefinition(Guid id, string key, string displayName, string? description, DataType dataType,
        ScopeType scopeType, string? unit)
        : base(id)
    {
        Key = key;
        DisplayName = displayName;
        Description = description;
        DataType = dataType;
        ScopeType = scopeType;
        Unit = unit;
        CreatedAt = DateTime.UtcNow;
    }

    public string Key { get; private set; }
    public string DisplayName { get; private set; }
    public string? Description { get; private set; }
    public DataType DataType { get; private set; }
    public ScopeType ScopeType { get; private set; }
    public string? Unit { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static AttributeDefinition Create(string key, string displayName, string? description, DataType dataType,
        ScopeType scopeType, string? unit)
    {
        return new AttributeDefinition(Guid.NewGuid(), key, displayName, description, dataType, scopeType, unit);
    }

    public void Update(string? displayName = null, string? description = null, DataType? dataType = null,
        ScopeType? scopeType = null, string? unit = null)
    {
        if (!string.IsNullOrWhiteSpace(displayName)) DisplayName = displayName;
        if (!string.IsNullOrWhiteSpace(description)) Description = description;
        if (dataType != null) DataType = dataType;
        if (scopeType != null) ScopeType = scopeType;
        if (!string.IsNullOrWhiteSpace(unit)) Unit = unit;
        UpdatedAt = DateTime.UtcNow;
    }
}