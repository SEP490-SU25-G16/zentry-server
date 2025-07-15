using Zentry.Modules.ConfigurationManagement.Dtos;

namespace Zentry.Modules.ConfigurationManagement.Features.GetConfigurations;

public class GetConfigurationsResponse
{
    public List<ConfigurationDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
