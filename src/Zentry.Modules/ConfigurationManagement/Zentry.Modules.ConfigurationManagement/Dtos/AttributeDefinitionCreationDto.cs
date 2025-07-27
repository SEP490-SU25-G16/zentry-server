using System.ComponentModel.DataAnnotations;

namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class AttributeDefinitionCreationDto
{
    public Guid? Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public List<OptionCreationDto>? Options { get; set; }
}
