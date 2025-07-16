using Microsoft.EntityFrameworkCore;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Integration;

public class GetSettingQueryHandler(
    ConfigurationDbContext dbContext,
    IRedisService redisService)
    : IQueryHandler<GetSettingsIntegrationQuery, GetSettingsIntegrationResponse>
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);

    public async Task<GetSettingsIntegrationResponse> Handle(GetSettingsIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        // Validate and parse ScopeType early
        var requestedScopeType = ValidateAndParseScopeType(query.ScopeType);

        // Generate cache key
        var cacheKey = GenerateCacheKey(query, requestedScopeType);

        // Try to get from cache first
        var cachedResponse = await redisService.GetAsync<GetSettingsIntegrationResponse>(cacheKey);
        if (cachedResponse != null) return cachedResponse;

        // Build and execute query
        var response = await ExecuteQueryAsync(query, requestedScopeType, cancellationToken);

        // Cache the response if applicable
        await TryCacheResponseAsync(cacheKey, response, query, requestedScopeType);

        return response;
    }

    private static ScopeType? ValidateAndParseScopeType(string? scopeType)
    {
        if (string.IsNullOrWhiteSpace(scopeType))
            return null;

        try
        {
            return ScopeType.FromName(scopeType);
        }
        catch (ArgumentException ex)
        {
            throw new BusinessLogicException($"Invalid ScopeType provided: {ex.Message}");
        }
    }

    private static string GenerateCacheKey(GetSettingsIntegrationQuery query, ScopeType? requestedScopeType)
    {
        var scopeTypeKey = requestedScopeType?.ToString() ?? "null";
        var scopeIdKey = query.ScopeId?.ToString() ?? "null";
        var keyParam = query.Key ?? "null";

        return $"settings:{scopeTypeKey}:{scopeIdKey}:{keyParam}:1:1";
    }

    private async Task<GetSettingsIntegrationResponse> ExecuteQueryAsync(
        GetSettingsIntegrationQuery query,
        ScopeType? requestedScopeType,
        CancellationToken cancellationToken)
    {
        var settingsQuery = BuildQuery(query, requestedScopeType);

        // Execute count and data queries in parallel for better performance
        var countTask = settingsQuery.CountAsync(cancellationToken);
        var dataTask = settingsQuery
            .OrderBy(c => c.CreatedAt)
            .Take(1)
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

        await Task.WhenAll(countTask, dataTask);

        return new GetSettingsIntegrationResponse
        {
            Items = await dataTask,
            TotalCount = await countTask,
            PageNumber = 1,
            PageSize = 1
        };
    }

    private IQueryable<Setting> BuildQuery(GetSettingsIntegrationQuery query, ScopeType? requestedScopeType)
    {
        var settingsQuery = dbContext.Settings
            .Include(c => c.AttributeDefinition)
            .AsQueryable();

        // Apply ScopeType filter
        if (requestedScopeType != null)
            settingsQuery = settingsQuery.Where(c => c.ScopeType == requestedScopeType);

        // Apply ScopeId filter
        if (query.ScopeId.HasValue)
            settingsQuery = settingsQuery.Where(c => c.ScopeId == query.ScopeId.Value);

        // Apply Key search filter
        if (!string.IsNullOrWhiteSpace(query.Key)) settingsQuery = ApplyKeyFilter(settingsQuery, query.Key);

        return settingsQuery;
    }

    private static IQueryable<Setting> ApplyKeyFilter(IQueryable<Setting> query, string searchKey)
    {
        var lowerSearchTerm = searchKey.ToLower();

        return query.Where(c =>
            (c.AttributeDefinition != null &&
             (c.AttributeDefinition.Key.ToLower().Contains(lowerSearchTerm) ||
              c.AttributeDefinition.DisplayName.ToLower().Contains(lowerSearchTerm))) ||
            c.Value.ToLower().Contains(lowerSearchTerm));
    }

    private async Task TryCacheResponseAsync(
        string cacheKey,
        GetSettingsIntegrationResponse response,
        GetSettingsIntegrationQuery query,
        ScopeType? requestedScopeType)
    {
        if (ShouldCacheResponse(query, requestedScopeType))
            await redisService.SetAsync(cacheKey, response, CacheExpiry);
    }

    private static bool ShouldCacheResponse(GetSettingsIntegrationQuery query, ScopeType? requestedScopeType)
    {
        // Don't cache search queries (queries with Key parameter)
        if (!string.IsNullOrWhiteSpace(query.Key))
            return false;

        // Only cache for specific scope types
        return requestedScopeType != null &&
               (requestedScopeType.Equals(ScopeType.GLOBAL) ||
                requestedScopeType.Equals(ScopeType.COURSE) ||
                requestedScopeType.Equals(ScopeType.SESSION));
    }
}
