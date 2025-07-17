namespace Zentry.SharedKernel.Contracts.Configuration;

public class GetSettingsByScopeIntegrationResponse
{
    public List<SettingContract> Items { get; set; } = [];
    public int TotalCount { get; set; }
    // Không cần PageNumber, PageSize vì không còn phân trang
}
