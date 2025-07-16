using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Configuration;

public record GetSettingsIntegrationQuery(
    string? Key,
    string? ScopeType,
    Guid? ScopeId
) : IQuery<GetSettingsIntegrationResponse>;