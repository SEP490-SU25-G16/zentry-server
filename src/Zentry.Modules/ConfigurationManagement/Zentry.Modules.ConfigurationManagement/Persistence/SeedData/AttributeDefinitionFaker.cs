using Bogus;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public class AttributeDefinitionFaker : Faker<AttributeDefinition>
{
    // Constructor cho các AttributeDefinition cố định, sử dụng FromSeedingData
    public AttributeDefinitionFaker(Guid id, string key, string displayName, string? description, DataType dataType, ScopeType scopeType, string? unit)
    {
        // Bogus sẽ gọi FromSeedingData để tạo đối tượng
        CustomInstantiator(f => AttributeDefinition.FromSeedingData(id, key, displayName, description, dataType, scopeType, unit));
    }

    // Constructor mặc định cho các AttributeDefinition ngẫu nhiên (nếu có)
    public AttributeDefinitionFaker()
    {
        RuleFor(ad => ad.Id, f => Guid.NewGuid());
        RuleFor(ad => ad.Key, f => f.Commerce.ProductName().Replace(" ", "").ToLowerInvariant());
        RuleFor(ad => ad.DisplayName, f => f.Commerce.ProductName());
        RuleFor(ad => ad.Description, f => f.Lorem.Sentence(5, 10));
        RuleFor(ad => ad.DataType, f => f.PickRandom<DataType>());
        RuleFor(ad => ad.ScopeType, f => f.PickRandom<ScopeType>());
        RuleFor(ad => ad.Unit, f => f.PickRandom<string>(null, "mét", "cm", "kg", "gam", "lít", "ml"));
        RuleFor(ad => ad.CreatedAt, f => f.Date.Past(1));
        RuleFor(ad => ad.UpdatedAt, f => f.Date.Recent(1));
    }
}
