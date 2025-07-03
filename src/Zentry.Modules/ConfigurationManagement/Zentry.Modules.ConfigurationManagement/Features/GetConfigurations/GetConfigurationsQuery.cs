using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ConfigurationManagement.Features.GetConfigurations;

public record GetConfigurationsQuery : IQuery<GetConfigurationsResponse>
{
    public Guid? AttributeId { get; init; }
    public string? ScopeTypeString { get; init; } // Đã đổi thành string, thêm hậu tố String để tránh nhầm lẫn
    public Guid? ScopeId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
