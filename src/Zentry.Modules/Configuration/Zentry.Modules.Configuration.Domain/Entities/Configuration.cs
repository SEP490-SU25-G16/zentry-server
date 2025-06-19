using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.Configuration.Domain.Entities;

public class Configuration : Entity
{
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Configuration() : base(Guid.Empty)
    {
    } // For EF Core

    public Configuration(string key, string value, string? description) : base(key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string newValue)
    {
        Value = newValue ?? throw new ArgumentNullException(nameof(newValue));
        UpdatedAt = DateTime.UtcNow;
    }
}
