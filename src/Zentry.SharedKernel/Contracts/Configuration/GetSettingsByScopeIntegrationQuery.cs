using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Configuration;

public record GetSettingsByScopeIntegrationQuery(
    string ScopeType
) : IQuery<GetSettingsByScopeIntegrationResponse>;
