using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ConfigurationManagement.Features.GetSettings;

public record GetSettingsQuery : IQuery<GetSettingsResponse>
{
    public Guid? AttributeId { get; init; }
    public string? ScopeTypeString { get; init; }
    public Guid? ScopeId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class GetSettingsResponse
{
    public List<SettingDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}