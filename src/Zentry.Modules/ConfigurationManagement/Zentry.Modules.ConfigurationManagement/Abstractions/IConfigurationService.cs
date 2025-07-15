using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.ConfigurationManagement.Abstractions;

public interface IConfigurationService
{
    Task<ConfigLookupResponseDto> GetConfigurationsAsync(ConfigLookupRequestDto requestDto);

    Task<string?> GetConfigurationValueAsync(string attributeKey, string scopeType, Guid scopeId);
}
