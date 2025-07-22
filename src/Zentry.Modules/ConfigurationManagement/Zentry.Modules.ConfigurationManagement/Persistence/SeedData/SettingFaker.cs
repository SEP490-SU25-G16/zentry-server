using Bogus;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public class SettingFaker : Faker<Setting>
{
    // Constructor cho việc tạo Setting cụ thể (sử dụng FromSeedingData)
    public SettingFaker(Guid attributeId, ScopeType scopeType, Guid scopeId, string value)
    {
        CustomInstantiator(f => Setting.FromSeedingData(Guid.NewGuid(), attributeId, scopeType, scopeId, value));
    }

    // Constructor mặc định cho Setting ngẫu nhiên
    public SettingFaker()
    {
        RuleFor(s => s.Id, f => Guid.NewGuid());
        RuleFor(s => s.AttributeId, f => Guid.NewGuid()); // Cần một AttributeId có thật nếu không truyền vào
        RuleFor(s => s.ScopeType, f => f.PickRandom<ScopeType>());
        RuleFor(s => s.ScopeId, f => Guid.NewGuid());
        RuleFor(s => s.Value, f => f.Random.Int(0, 100).ToString());
        RuleFor(s => s.CreatedAt, f => f.Date.Past(1));
        RuleFor(s => s.UpdatedAt, f => f.Date.Recent(1));
    }
}
