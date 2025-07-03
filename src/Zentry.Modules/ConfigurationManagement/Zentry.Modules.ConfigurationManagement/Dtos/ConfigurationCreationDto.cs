using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class ConfigurationCreationDto
{
    public string ScopeType { get; set; } = string.Empty; // Đã đổi thành string
    public Guid ScopeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
