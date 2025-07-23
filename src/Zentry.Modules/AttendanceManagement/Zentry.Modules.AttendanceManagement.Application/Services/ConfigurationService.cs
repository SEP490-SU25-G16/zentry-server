using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.SharedKernel.Contracts.Configuration;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public static class AttendanceScopeTypes
{
    public const string Global = "GLOBAL";
    public const string Course = "COURSE";
    public const string Session = "SESSION";
}

public class ConfigurationService(
    IMediator mediator,
    IRedisService redisService,
    ILogger<ConfigurationService> logger)
    : IConfigurationService
{
    private readonly TimeSpan _localCacheExpiry = TimeSpan.FromMinutes(30);

    public async Task<GetMultipleSettingsIntegrationResponse> GetMultipleSettingsInBatchAsync(
        List<ScopeQueryRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMultipleSettingsIntegrationQuery(requests);
        return await mediator.Send(query, cancellationToken);
    }

    public async Task<Dictionary<string, SettingContract>> GetAllSettingsForScopeAsync(
        string scopeType,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default)
    {
        // Tạo cache key phức hợp để bao gồm cả ScopeType và ScopeId
        // Điều này đảm bảo rằng mỗi tập hợp settings cho một scope cụ thể sẽ có cache key riêng
        var cacheKey = $"appsettings:all:{scopeType.ToLower()}:{scopeId?.ToString() ?? "null"}";

        // 1. Thử lấy từ cache cục bộ của Attendance Module
        var cachedSettings = await redisService.GetAsync<Dictionary<string, SettingContract>>(cacheKey);
        if (cachedSettings != null)
        {
            logger.LogDebug("Cache hit for all settings in scope '{ScopeType}' (ID: {ScopeId}).", scopeType, scopeId);
            return cachedSettings;
        }

        logger.LogDebug(
            "Local cache miss for all settings in scope '{ScopeType}' (ID: {ScopeId}). Fetching from Setting Module.",
            scopeType, scopeId);

        // 2. GỌI GetSettingsByScopeIntegrationQuery từ Setting Module
        // SỬ DỤNG GetSettingsByScopeIntegrationQuery MỚI
        var request = new GetSettingsByScopeIntegrationQuery(scopeType);

        try
        {
            var response = await mediator.Send(request, cancellationToken);

            // Filter by ScopeId if provided and not Global scope
            var filteredItems = response.Items.AsEnumerable();
            if (scopeId.HasValue && scopeId.Value != Guid.Empty &&
                !scopeType.Equals(AttendanceScopeTypes.Global, StringComparison.OrdinalIgnoreCase))
                filteredItems = filteredItems.Where(s => s.ScopeId == scopeId.Value);

            var settingsDictionary = filteredItems.ToDictionary(
                s => s.AttributeKey,
                s => s, // Lưu toàn bộ SettingContract để có thể truy cập Value, DataType, v.v.
                StringComparer.OrdinalIgnoreCase
            );

            // 3. Cache vào Redis cục bộ nếu là scope type được phép và có dữ liệu
            if (settingsDictionary.Count == 0 ||
                (!scopeType.Equals(AttendanceScopeTypes.Global, StringComparison.OrdinalIgnoreCase) &&
                 !scopeType.Equals(AttendanceScopeTypes.Course, StringComparison.OrdinalIgnoreCase) &&
                 !scopeType.Equals(AttendanceScopeTypes.Session, StringComparison.OrdinalIgnoreCase)))
                return settingsDictionary;
            await redisService.SetAsync(cacheKey, settingsDictionary, _localCacheExpiry);
            logger.LogInformation("Cached all settings for scope '{ScopeType}' (ID: {ScopeId}) locally.", scopeType,
                scopeId);

            return settingsDictionary;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve all settings for scope '{ScopeType}' (ID: {ScopeId}) from Setting Module.",
                scopeType, scopeId);
            // Quan trọng: Trả về một Dictionary rỗng để tránh NullReferenceException
            // Hoặc throw lại nếu bạn muốn lỗi dừng quá trình.
            return new Dictionary<string, SettingContract>();
        }
    }
}