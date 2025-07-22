using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Configuration;

public record ScopeQueryRequest(string ScopeType, Guid? ScopeId);

public record GetMultipleSettingsIntegrationResponse(
    Dictionary<string, List<SettingContract>> SettingsByScopeType
);

// Query chính
public record GetMultipleSettingsIntegrationQuery(
    List<ScopeQueryRequest> Requests
) : IQuery<GetMultipleSettingsIntegrationResponse>;
