using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateConfiguration;

public class CreateConfigurationCommand : ICommand<CreateConfigurationResponse>
{
    public AttributeDefinitionCreationDto? AttributeDefinitionDetails { get; set; }
    public ConfigurationCreationDto Configuration { get; set; } = new();
}
