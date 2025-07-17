using Zentry.SharedKernel.Contracts.Configuration;

namespace Zentry.Modules.AttendanceManagement.Application.Services.Interface;

public interface IAppConfigurationService
{
    Task<Dictionary<string, SettingContract>> GetAllSettingsForScopeAsync(
        string scopeType,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default);
}
