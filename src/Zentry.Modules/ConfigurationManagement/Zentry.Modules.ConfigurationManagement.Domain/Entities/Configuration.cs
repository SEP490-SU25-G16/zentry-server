using Zentry.Modules.ConfigurationManagement.Domain.Enums;
using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Domain.Entities;

public class Configuration : AggregateRoot<Guid>
{
    private Configuration() : base(Guid.Empty) { }
    private Configuration(Guid id, Guid attributeId, ScopeType scopeType, Guid scopeId, string value)
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
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Configuration Create(Guid attributeId, ScopeType scopeType, Guid scopeId, string value)
    {
        return new Configuration(Guid.NewGuid(), attributeId, scopeType, scopeId, value);
    }

    public void UpdateValue(string newValue)
    {
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
    }
}
