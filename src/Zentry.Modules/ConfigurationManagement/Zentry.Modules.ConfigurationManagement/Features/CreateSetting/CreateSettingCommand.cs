using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Configuration;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateSetting;

public class CreateSettingCommand : ICommand<CreateSettingResponse>
{
    public AttributeDefinitionCreationDto? AttributeDefinitionDetails { get; set; }
    public SettingCreationDto Setting { get; set; } = new();
}

public class CreateSettingResponse
{
    public Guid SettingId { get; set; }

    // Thông tin của AttributeDefinition đã được tạo/cập nhật
    public Guid AttributeId { get; set; }
    public string AttributeKey { get; set; } = string.Empty;
    public string AttributeDisplayName { get; set; } = string.Empty;
    public DataType DataType { get; set; }
    public ScopeType AttributeDefinitionScopeType { get; set; } // ScopeType từ AttributeDefinition
    public string? Unit { get; set; }

    // Thông tin về Options (nếu có và DataType là Selection)
    public List<OptionDto>? Options { get; set; } // Cần tạo OptionDto riêng

    // Thông tin của Setting
    public ScopeType SettingScopeType { get; set; } // ScopeType từ Setting
    public Guid ScopeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}