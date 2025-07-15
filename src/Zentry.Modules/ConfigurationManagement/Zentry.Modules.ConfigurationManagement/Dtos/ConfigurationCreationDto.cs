namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class ConfigurationCreationDto
{
    public string ScopeType { get; set; } = string.Empty; 
    public Guid ScopeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
