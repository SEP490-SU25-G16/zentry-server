using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateSetting;

public class CreateSettingCommand : ICommand<CreateSettingResponse>
{
    public AttributeDefinitionCreationDto? AttributeDefinitionDetails { get; set; }
    public SettingCreationDto Setting { get; set; } = new();
}
