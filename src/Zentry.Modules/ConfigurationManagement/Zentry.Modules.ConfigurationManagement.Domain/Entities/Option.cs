using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Domain.Entities;

public class Option : AggregateRoot<Guid>
{
    private Option() : base(Guid.Empty)
    {
    }

    private Option(Guid id, Guid attributeId, string value, string displayLabel, int sortOrder)
        : base(id)
    {
        AttributeId = attributeId;
        Value = value;
        DisplayLabel = displayLabel;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid AttributeId { get; private set; }
    public string Value { get; private set; }
    public string DisplayLabel { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Option Create(Guid attributeId, string value, string displayLabel, int sortOrder)
    {
        return new Option(Guid.NewGuid(), attributeId, value, displayLabel, sortOrder);
    }

    public void Update(string? value = null, string? displayLabel = null, int? sortOrder = null)
    {
        if (!string.IsNullOrWhiteSpace(value)) Value = value;
        if (!string.IsNullOrWhiteSpace(displayLabel)) DisplayLabel = displayLabel;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}