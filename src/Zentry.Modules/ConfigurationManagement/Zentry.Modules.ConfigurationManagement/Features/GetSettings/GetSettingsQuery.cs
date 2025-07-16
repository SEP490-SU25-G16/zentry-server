using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ConfigurationManagement.Features.GetConfigurations;

public record GetSettingsQuery : IQuery<GetSettingsResponse>
{
    public Guid? AttributeId { get; init; }
    public string? ScopeTypeString { get; init; }
    public Guid? ScopeId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
