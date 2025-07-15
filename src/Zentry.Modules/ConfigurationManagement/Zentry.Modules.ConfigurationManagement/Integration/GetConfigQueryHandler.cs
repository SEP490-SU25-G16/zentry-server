using Microsoft.EntityFrameworkCore;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Integration;

public class GetConfigQueryHandler(
    ConfigurationDbContext dbContext,
    IRedisService redisService)
    : IQueryHandler<GetConfigurationsIntegrationQuery, GetConfigurationsIntegrationResponse>
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);

    public async Task<GetConfigurationsIntegrationResponse> Handle(GetConfigurationsIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        // Validate and parse ScopeType early
        var requestedScopeType = ValidateAndParseScopeType(query.ScopeType);

        // Generate cache key
        var cacheKey = GenerateCacheKey(query, requestedScopeType);

        // Try to get from cache first
        var cachedResponse = await redisService.GetAsync<GetConfigurationsIntegrationResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

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

    private static string GenerateCacheKey(GetConfigurationsIntegrationQuery query, ScopeType? requestedScopeType)
    {
        var scopeTypeKey = requestedScopeType?.ToString() ?? "null";
        var scopeIdKey = query.ScopeId?.ToString() ?? "null";
        var keyParam = query.Key ?? "null";

        return $"configurations:{scopeTypeKey}:{scopeIdKey}:{keyParam}:1:1";
    }

    private async Task<GetConfigurationsIntegrationResponse> ExecuteQueryAsync(
        GetConfigurationsIntegrationQuery query,
        ScopeType? requestedScopeType,
        CancellationToken cancellationToken)
    {
        var configurationsQuery = BuildQuery(query, requestedScopeType);

        // Execute count and data queries in parallel for better performance
        var countTask = configurationsQuery.CountAsync(cancellationToken);
        var dataTask = configurationsQuery
            .OrderBy(c => c.CreatedAt)
            .Take(1)
            .Select(c => new ConfigurationContract
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

        return new GetConfigurationsIntegrationResponse
        {
            Items = await dataTask,
            TotalCount = await countTask,
            PageNumber = 1,
            PageSize = 1
        };
    }

    private IQueryable<Configuration> BuildQuery(GetConfigurationsIntegrationQuery query, ScopeType? requestedScopeType)
    {
        var configurationsQuery = dbContext.Configurations
            .Include(c => c.AttributeDefinition)
            .AsQueryable();

        // Apply ScopeType filter
        if (requestedScopeType != null)
        {
            configurationsQuery = configurationsQuery.Where(c => c.ScopeType == requestedScopeType);
        }

        // Apply ScopeId filter
        if (query.ScopeId.HasValue)
        {
            configurationsQuery = configurationsQuery.Where(c => c.ScopeId == query.ScopeId.Value);
        }

        // Apply Key search filter
        if (!string.IsNullOrWhiteSpace(query.Key))
        {
            configurationsQuery = ApplyKeyFilter(configurationsQuery, query.Key);
        }

        return configurationsQuery;
    }

    private static IQueryable<Configuration> ApplyKeyFilter(IQueryable<Configuration> query, string searchKey)
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
        GetConfigurationsIntegrationResponse response,
        GetConfigurationsIntegrationQuery query,
        ScopeType? requestedScopeType)
    {
        if (ShouldCacheResponse(query, requestedScopeType))
        {
            await redisService.SetAsync(cacheKey, response, CacheExpiry);
        }
    }

    private static bool ShouldCacheResponse(GetConfigurationsIntegrationQuery query, ScopeType? requestedScopeType)
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
