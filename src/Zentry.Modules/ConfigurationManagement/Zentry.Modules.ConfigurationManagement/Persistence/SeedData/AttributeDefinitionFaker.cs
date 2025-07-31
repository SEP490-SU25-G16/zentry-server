using Bogus;
using Zentry.Modules.ConfigurationManagement.Entities;
using Zentry.SharedKernel.Constants.Configuration;

// Thêm namespace này

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public class AttributeDefinitionFaker : Faker<AttributeDefinition>
{
    public AttributeDefinitionFaker(Guid id, string key, string displayName, string? description, DataType dataType,
        List<ScopeType> allowedScopeTypes, string? unit, string? defaultValue,
        bool isDeletable = true) // <-- Cập nhật tham số
    {
        CustomInstantiator(f =>
            AttributeDefinition.FromSeedingData(id, key, displayName, description, dataType, allowedScopeTypes, unit,
                defaultValue, isDeletable)); // <-- Cập nhật tham số
    }

    public AttributeDefinitionFaker()
    {
        RuleFor(ad => ad.Id, f => Guid.NewGuid());
        RuleFor(ad => ad.Key, f => f.Commerce.ProductName().Replace(" ", "").ToLowerInvariant());
        RuleFor(ad => ad.DisplayName, f => f.Commerce.ProductName());
        RuleFor(ad => ad.Description, f => f.Lorem.Sentence(5, 10));
        RuleFor(ad => ad.DataType, f => f.PickRandom<DataType>());
        RuleFor(ad => ad.AllowedScopeTypes,
            f => f.PickRandom(ScopeType.GetAll<ScopeType>(), f.Random.Int(1, 3))
                .ToList()); // Chọn 1-3 loại scope ngẫu nhiên
        RuleFor(ad => ad.Unit, f => f.PickRandom<string>(null, "mét", "cm", "kg", "gam", "lít", "ml"));
        RuleFor(ad => ad.DefaultValue, f => f.Random.String2(5, 20));
        RuleFor(ad => ad.IsDeletable, f => f.Random.Bool());
        RuleFor(ad => ad.CreatedAt, f => f.Date.Past());
        RuleFor(ad => ad.UpdatedAt, f => f.Date.Recent());
    }
}