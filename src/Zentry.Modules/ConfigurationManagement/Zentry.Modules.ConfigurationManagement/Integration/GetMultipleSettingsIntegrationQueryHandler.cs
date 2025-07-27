using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Configuration;
using Zentry.SharedKernel.Contracts.Configuration;

namespace Zentry.Modules.ConfigurationManagement.Integration;

public class GetMultipleSettingsIntegrationQueryHandler(
    ConfigurationDbContext dbContext,
    IRedisService redisService,
    ILogger<GetMultipleSettingsIntegrationQueryHandler> logger)
    : IQueryHandler<GetMultipleSettingsIntegrationQuery, GetMultipleSettingsIntegrationResponse>
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);

    public async Task<GetMultipleSettingsIntegrationResponse> Handle(
        GetMultipleSettingsIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        // Dictionary để lưu trữ kết quả cho từng ScopeType
        var settingsByScopeType = new Dictionary<string, List<SettingContract>>();
        var scopeTypesToFetchFromDb = new HashSet<ScopeType>();

        // Lọc các request để xác định những gì cần truy vấn
        foreach (var request in query.Requests)
        {
            ScopeType parsedScopeType;
            try
            {
                parsedScopeType = ScopeType.FromName(request.ScopeType);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning("Invalid ScopeType '{ScopeType}' in GetMultipleSettingsIntegrationQuery: {Message}",
                    request.ScopeType, ex.Message);
                continue; // Bỏ qua request không hợp lệ
            }

            var cacheKey = GenerateCacheKey(parsedScopeType, request.ScopeId); // Cache key bao gồm ScopeId
            var cachedSettings = await redisService.GetAsync<List<SettingContract>>(cacheKey);

            if (cachedSettings != null)
            {
                logger.LogDebug("Integration Cache hit for key: {CacheKey}", cacheKey);
                settingsByScopeType[request.ScopeType] = cachedSettings;
            }
            else
            {
                logger.LogDebug("Integration Cache miss for key: {CacheKey}. Marking for DB fetch.", cacheKey);
                scopeTypesToFetchFromDb.Add(parsedScopeType); // Đánh dấu để fetch từ DB
            }
        }

        if (scopeTypesToFetchFromDb.Count != 0)
        {
            logger.LogInformation("Fetching settings for {ScopeTypes} from DB.",
                string.Join(", ", scopeTypesToFetchFromDb));

            // Xây dựng truy vấn để lấy TẤT CẢ settings cho các ScopeType đã đánh dấu
            var dbQuery = dbContext.Settings
                .Include(s => s.AttributeDefinition)
                .Where(s => scopeTypesToFetchFromDb.Contains(s.ScopeType))
                .AsNoTracking();

            var allFetchedSettings = await dbQuery.ToListAsync(cancellationToken);

            // Phân loại kết quả và xử lý cache
            foreach (var requestedScope in query.Requests)
            {
                var parsedScopeType =
                    ScopeType.FromName(requestedScope.ScopeType);
                var cacheKey = GenerateCacheKey(parsedScopeType, requestedScope.ScopeId);

                // Nếu kết quả cho scope này chưa có từ cache
                if (!settingsByScopeType.ContainsKey(requestedScope.ScopeType))
                {
                    var filteredSettings = allFetchedSettings
                        .Where(s => s.ScopeType == parsedScopeType &&
                                    (requestedScope.ScopeId == null || s.ScopeId == requestedScope.ScopeId))
                        .Select(s => new SettingContract
                        {
                            Id = s.Id,
                            AttributeId = s.AttributeId,
                            AttributeKey = s.AttributeDefinition?.Key ?? "N/A",
                            AttributeDisplayName = s.AttributeDefinition?.DisplayName ?? "N/A",
                            DataType = s.AttributeDefinition?.DataType.ToString() ?? "N/A",
                            ScopeType = s.ScopeType.ToString(),
                            ScopeId = s.ScopeId,
                            Value = s.Value,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt
                        })
                        .ToList();

                    settingsByScopeType[requestedScope.ScopeType] = filteredSettings;

                    await TryCacheResponseAsync(cacheKey, filteredSettings, parsedScopeType);
                }
            }
        }
        else
        {
            logger.LogInformation("All requested settings were found in cache. No DB fetch needed.");
        }

        return new GetMultipleSettingsIntegrationResponse(settingsByScopeType);
    }

    private static string GenerateCacheKey(ScopeType requestedScopeType, Guid? scopeId)
    {
        // Cache key bây giờ phải bao gồm cả ScopeId để phân biệt cài đặt GLOBAL với GLOBAL của một Course/Session cụ thể (nếu có)
        // Ví dụ: "Settings:integration:GLOBAL:null", "Settings:integration:COURSE:{courseId}", "Settings:integration:SESSION:{sessionId}"
        return $"Settings:integration:{requestedScopeType}:{scopeId?.ToString() ?? "null"}";
    }

    private async Task TryCacheResponseAsync(
        string cacheKey,
        List<SettingContract> settingsToCache,
        ScopeType requestedScopeType)
    {
        // Chỉ cache những loại scope nhất định
        if (requestedScopeType.Equals(ScopeType.Global) ||
            requestedScopeType.Equals(ScopeType.Course) ||
            requestedScopeType.Equals(ScopeType.Session))
        {
            await redisService.SetAsync(cacheKey, settingsToCache, CacheExpiry);
            logger.LogInformation("Cached response for key: {CacheKey}", cacheKey);
        }
        else
        {
            logger.LogDebug("Not caching response for ScopeType: {RequestedScopeType} (not configured for caching).",
                requestedScopeType);
        }
    }
}
