using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Configuration;

public record GetConfigurationsIntegrationQuery(
    string? Key,
    string? ScopeType,
    Guid? ScopeId
) : IQuery<GetConfigurationsIntegrationResponse>;
