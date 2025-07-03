using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class GetConfigurationsRequest
{
    public Guid? AttributeId { get; init; }
    public string? ScopeType { get; init; } // Đã đổi thành string
    public Guid? ScopeId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1; // Giá trị mặc định
    public int PageSize { get; init; } = 10; // Giá trị mặc định
}
