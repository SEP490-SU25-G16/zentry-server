using Bogus;
using Zentry.Modules.ConfigurationManagement.Entities;

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public class OptionFaker : Faker<Option>
{
    // Constructor cho việc tạo Options cụ thể (ví dụ: cho các AttributeId cố định)
    public OptionFaker(Guid attributeId, string value, string displayLabel, int sortOrder)
    {
        CustomInstantiator(f => Option.FromSeedingData(Guid.NewGuid(), attributeId, value, displayLabel, sortOrder));
    }

    // Constructor mặc định cho Options ngẫu nhiên
    public OptionFaker()
    {
        RuleFor(o => o.Id, f => Guid.NewGuid());
        RuleFor(o => o.AttributeId, f => Guid.NewGuid()); // Cần một AttributeId có thật nếu không truyền vào
        RuleFor(o => o.Value, f => f.Random.Word());
        RuleFor(o => o.DisplayLabel, f => f.Commerce.ProductAdjective());
        RuleFor(o => o.SortOrder, f => f.Random.Int(1, 100));
        RuleFor(o => o.CreatedAt, f => f.Date.Past());
        RuleFor(o => o.UpdatedAt, f => f.Date.Recent());
    }
}