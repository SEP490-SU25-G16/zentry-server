using System.ComponentModel.DataAnnotations;
using Zentry.SharedKernel.Helpers;

namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class SettingCreationDto
{
    public string ScopeType { get; set; } = string.Empty;
    public Guid ScopeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
