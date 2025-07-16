namespace Zentry.SharedKernel.Contracts.Configuration;

public class GetSettingsIntegrationResponse
{
    public List<SettingContract> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
