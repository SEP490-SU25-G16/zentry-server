using Zentry.SharedKernel.Common;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.Configuration.Domain.Entities;

public class Configuration : AggregateRoot
{
    private Configuration() : base(Guid.Empty)
    {
        Key = string.Empty;
        Value = string.Empty;
    } // For EF Core

    private Configuration(Guid configurationId, string key, string value, string? description)
        : base(configurationId)
    {
        Guard.AgainstNull(key, nameof(key));
        Guard.AgainstNull(value, nameof(value));

        ConfigurationId = configurationId;
        Key = key;
        Value = value;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ConfigurationId { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Configuration Create(string key, string value, string? description = null)
    {
        return new Configuration(Guid.NewGuid(), key, value, description);
    }

    public void UpdateValue(string newValue, string? newDescription = null)
    {
        Guard.AgainstNull(newValue, nameof(newValue));

        Value = newValue;
        if (newDescription != null)
        {
            Description = newDescription;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }
}
