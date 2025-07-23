using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Persistence.Entities;

public class Setting : AggregateRoot<Guid>
{
    public Setting() : base(Guid.Empty)
    {
        Value = string.Empty;
        ScopeType = ScopeType.GLOBAL; // Default
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private Setting(Guid id, Guid attributeId, ScopeType scopeType, Guid scopeId, string value)
        : base(id)
    {
        AttributeId = attributeId;
        ScopeType = scopeType;
        ScopeId = scopeId;
        Value = value;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid AttributeId { get; private set; }
    public ScopeType ScopeType { get; private set; }
    public Guid ScopeId { get; private set; }
    public string Value { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; }
    public virtual AttributeDefinition AttributeDefinition { get; private set; }

    public static Setting Create(Guid attributeId, ScopeType scopeType, Guid scopeId, string value)
    {
        return new Setting(Guid.NewGuid(), attributeId, scopeType, scopeId, value);
    }

    // Thêm phương thức FromSeedingData
    public static Setting FromSeedingData(Guid id, Guid attributeId, ScopeType scopeType, Guid scopeId, string value)
    {
        return new Setting(id, attributeId, scopeType, scopeId, value);
    }

    public void UpdateValue(string newValue)
    {
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
    }
}