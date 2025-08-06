using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Entities;

public class UserAttribute : AggregateRoot<Guid>
{
    private UserAttribute() : base(Guid.Empty)
    {
    }

    private UserAttribute(Guid id, Guid userId, Guid attributeId, string attributeValue)
        : base(id)
    {
        UserId = userId;
        AttributeId = attributeId;
        AttributeValue = attributeValue;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid AttributeId { get; private set; }
    public string AttributeValue { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static UserAttribute Create(Guid userId, Guid attributeId, string attributeValue)
    {
        return new UserAttribute(Guid.NewGuid(), userId, attributeId, attributeValue);
    }

    public void UpdateValue(string newAttributeValue)
    {
        AttributeValue = newAttributeValue;
        UpdatedAt = DateTime.UtcNow;
    }
}