using Microsoft.EntityFrameworkCore;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Features.GetConfigurations;

public class
    GetConfigurationsQueryHandler(
        ConfigurationDbContext dbContext,
        IRedisService redisService)
    : IQueryHandler<GetConfigurationsQuery, GetConfigurationsResponse>
{
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);

    public async Task<GetConfigurationsResponse> Handle(GetConfigurationsQuery query,
        CancellationToken cancellationToken)
    {
        // Tạo cache key dựa trên các tham số query
        // Đảm bảo cache key đủ độc đáo cho mỗi loại query
        var cacheKey =
            $"configurations:{query.AttributeId?.ToString() ?? "null"}:{query.ScopeTypeString ?? "null"}:{query.ScopeId?.ToString() ?? "null"}:{query.SearchTerm ?? "null"}:{query.PageNumber}:{query.PageSize}";

        // 1. Thử lấy từ Redis trước
        var cachedResponse = await redisService.GetAsync<GetConfigurationsResponse>(cacheKey);
        if (cachedResponse != null)
        {
            // Log for debugging (optional)
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedResponse;
        }

        // Log for debugging (optional)
        Console.WriteLine($"Cache miss for key: {cacheKey}. Fetching from DB...");

        IQueryable<Configuration> configurationsQuery = dbContext.Configurations
            .Include(c => c.AttributeDefinition);

        if (query.AttributeId.HasValue)
            configurationsQuery = configurationsQuery.Where(c => c.AttributeId == query.AttributeId.Value);

        ScopeType? requestedScopeType = null;
        // Chuyển đổi string ScopeTypeString từ query sang Smart Enum
        if (!string.IsNullOrWhiteSpace(query.ScopeTypeString))
        {
            try
            {
                requestedScopeType = ScopeType.FromName(query.ScopeTypeString);
                configurationsQuery = configurationsQuery.Where(c => c.ScopeType == requestedScopeType);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessLogicException($"Invalid ScopeType provided: {ex.Message}");
            }
        }

        if (query.ScopeId.HasValue)
            configurationsQuery = configurationsQuery.Where(c => c.ScopeId == query.ScopeId.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var lowerSearchTerm = query.SearchTerm.ToLower();
            configurationsQuery = configurationsQuery.Where(c =>
                (c.AttributeDefinition != null && (c.AttributeDefinition.Key.ToLower().Contains(lowerSearchTerm) ||
                                                   c.AttributeDefinition.DisplayName.ToLower()
                                                       .Contains(lowerSearchTerm))) ||
                c.Value.ToLower().Contains(lowerSearchTerm));
        }

        var totalCount = await configurationsQuery.CountAsync(cancellationToken);

        var configurations = await configurationsQuery
            .OrderBy(c => c.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var configDtos = configurations.Select(c => new ConfigurationDto
        {
            Id = c.Id,
            AttributeId = c.AttributeId,
            AttributeKey = c.AttributeDefinition?.Key ?? "N/A",
            AttributeDisplayName = c.AttributeDefinition?.DisplayName ?? "N/A",
            DataType = c.AttributeDefinition?.DataType != null ? c.AttributeDefinition.DataType.ToString() : "N/A",
            ScopeType = c.ScopeType != null ? c.ScopeType.ToString() : "N/A",
            ScopeId = c.ScopeId,
            Value = c.Value,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        var response = new GetConfigurationsResponse
        {
            Items = configDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        // 2. Lưu vào Redis nếu ScopeType phù hợp và không có SearchTerm (để tránh cache quá nhiều data linh tinh)
        // Chỉ cache nếu không có searchTerm, và ScopeType là GLOBAL, COURSE, hoặc SESSION
        var canCache = string.IsNullOrWhiteSpace(query.SearchTerm) &&
                       (requestedScopeType == ScopeType.GLOBAL ||
                        requestedScopeType == ScopeType.COURSE ||
                        requestedScopeType ==
                        ScopeType
                            .SESSION); // Sử dụng `SESSION` như định nghĩa của bạn cho `ScopeType` là lịch trình/buổi học

        if (canCache)
        {
            await redisService.SetAsync(cacheKey, response, _cacheExpiry);
            Console.WriteLine($"Cached response for key: {cacheKey}"); // Log for debugging
        }

        return response;
    }
}
