using Microsoft.EntityFrameworkCore;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Configuration;
using Zentry.SharedKernel.Contracts.Configuration;

namespace Zentry.Modules.ConfigurationManagement.Integration;

public class GetSettingsByScopeQueryHandler(
    ConfigurationDbContext dbContext,
    IRedisService redisService)
    : IQueryHandler<GetSettingsByScopeIntegrationQuery, GetSettingsByScopeIntegrationResponse>
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);

    public async Task<GetSettingsByScopeIntegrationResponse> Handle(GetSettingsByScopeIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        // 1. Validate và Parse ScopeType
        // ScopeType là bắt buộc, nên sẽ không có trường hợp null
        var requestedScopeType = ValidateAndParseScopeType(query.ScopeType);

        // 2. Tạo Cache Key
        // Cache key chỉ dựa vào ScopeType
        var cacheKey = GenerateCacheKey(requestedScopeType);

        // 3. Thử lấy từ Redis Cache
        var cachedResponse = await redisService.GetAsync<GetSettingsByScopeIntegrationResponse>(cacheKey);
        if (cachedResponse != null)
        {
            Console.WriteLine($"Integration Cache hit for key: {cacheKey}");
            return cachedResponse;
        }

        Console.WriteLine($"Integration Cache miss for key: {cacheKey}. Fetching from DB...");

        // 4. Xây dựng và thực thi truy vấn để lấy TẤT CẢ cấu hình theo ScopeType
        var response = await ExecuteQueryAsync(requestedScopeType, cancellationToken);

        // 5. Lưu vào Redis Cache (nếu ScopeType hợp lệ)
        await TryCacheResponseAsync(cacheKey, response, requestedScopeType);

        return response;
    }

    private static ScopeType ValidateAndParseScopeType(string scopeTypeString) // Không còn '?''
    {
        if (string.IsNullOrWhiteSpace(scopeTypeString))
            throw new ArgumentException(
                "ScopeType cannot be null or empty for GetSettingsByScopeIntegrationQuery.");

        try
        {
            return ScopeType.FromName(scopeTypeString);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid ScopeType provided for integration query: {ex.Message}");
        }
    }

    private static string GenerateCacheKey(ScopeType requestedScopeType)
    {
        return $"Settings:integration:all_by_scope:{requestedScopeType}";
    }

    private async Task<GetSettingsByScopeIntegrationResponse> ExecuteQueryAsync(
        ScopeType requestedScopeType,
        CancellationToken cancellationToken)
    {
        var SettingsQuery = dbContext.Settings
            .Include(c => c.AttributeDefinition)
            .Where(c => c.ScopeType == requestedScopeType)
            .AsNoTracking();

        var data = await SettingsQuery
            .OrderBy(c => c.AttributeDefinition!.Key)
            .Select(c => new SettingContract
            {
                Id = c.Id,
                AttributeId = c.AttributeId,
                AttributeKey = c.AttributeDefinition != null ? c.AttributeDefinition.Key : "N/A",
                AttributeDisplayName = c.AttributeDefinition != null ? c.AttributeDefinition.DisplayName : "N/A",
                DataType = c.AttributeDefinition != null ? c.AttributeDefinition.DataType.ToString() : "N/A",
                ScopeType = c.ScopeType != null ? c.ScopeType.ToString() : "N/A",
                ScopeId = c.ScopeId,
                Value = c.Value,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetSettingsByScopeIntegrationResponse
        {
            Items = data,
            TotalCount = data.Count
        };
    }

    private async Task TryCacheResponseAsync(
        string cacheKey,
        GetSettingsByScopeIntegrationResponse response,
        ScopeType requestedScopeType)
    {
        if (requestedScopeType.Equals(ScopeType.Global) ||
            requestedScopeType.Equals(ScopeType.Course) ||
            requestedScopeType.Equals(ScopeType.Session))
        {
            await redisService.SetAsync(cacheKey, response, CacheExpiry);
            Console.WriteLine($"Cached response for key: {cacheKey}");
        }
        else
        {
            Console.WriteLine(
                $"Not caching response for ScopeType: {requestedScopeType} (not configured for caching).");
        }
    }
}