using Microsoft.EntityFrameworkCore;
using Zentry.Infrastructure.Caching; // Đảm bảo namespace này đúng
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Configuration;
using System.Linq;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Exceptions; // Thêm namespace này

namespace Zentry.Modules.ConfigurationManagement.Features.GetSettings;

public class
    GetSettingsQueryHandler(
        ConfigurationDbContext dbContext,
        IRedisService redisService)
    : IQueryHandler<GetSettingsQuery, GetSettingsResponse>
{
    public async Task<GetSettingsResponse> Handle(GetSettingsQuery query,
        CancellationToken cancellationToken)
    {
        // Tạo cache key dựa trên các tham số query
        var cacheKey =
            $"settings:{query.AttributeId?.ToString() ?? "null"}:{query.ScopeType ?? "null"}:{query.ScopeId ?? "null"}:{query.SearchTerm ?? "null"}:{query.PageNumber}:{query.PageSize}";

        // 1. Thử lấy từ Redis trước
        var cachedResponse = await redisService.GetAsync<GetSettingsResponse>(cacheKey);
        if (cachedResponse != null)
        {
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedResponse;
        }

        Console.WriteLine($"Cache miss for key: {cacheKey}. Fetching from DB...");

        IQueryable<Setting> settingsQuery = dbContext.Settings
            .Include(c => c.AttributeDefinition);

        if (query.AttributeId.HasValue)
            settingsQuery = settingsQuery.Where(c => c.AttributeId == query.AttributeId.Value);

        ScopeType? requestedScopeType = null;
        Guid? parsedScopeId = null; // Biến để lưu ScopeId đã parse

        // Chuyển đổi string ScopeType từ query sang Smart Enum
        if (!string.IsNullOrWhiteSpace(query.ScopeType))
            try
            {
                requestedScopeType = ScopeType.FromName(query.ScopeType);
                settingsQuery = settingsQuery.Where(c => c.ScopeType == requestedScopeType);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidSettingValueException(ErrorMessages.Settings.InvalidSettingValue);
            }

        // Xử lý ScopeId dựa trên ScopeType và giá trị chuỗi
        if (!string.IsNullOrWhiteSpace(query.ScopeId))
        {
            // Cố gắng parse ScopeId nếu nó không rỗng
            if (Guid.TryParse(query.ScopeId, out var tempGuid))
            {
                parsedScopeId = tempGuid;
            }
            else
            {
                // Nếu không parse được và nó không rỗng, đây là lỗi định dạng GUID
                // Throw một exception để validator bắt hoặc xử lý ở đây
                throw new ArgumentException("ScopeId không phải là định dạng GUID hợp lệ.");
            }
        }
        else // Nếu ScopeId là null hoặc chuỗi rỗng
        {
            parsedScopeId = Guid.Empty; // Coi null/rỗng là Guid.Empty cho mục đích truy vấn
        }

        // Áp dụng bộ lọc ScopeId đã parse
        // Điều chỉnh logic lọc ScopeId để phù hợp với quy tắc Global/non-Global
        // Query theo parsedScopeId thay vì query.ScopeId.Value
        if (requestedScopeType == ScopeType.Global)
        {
            // Với Global, ScopeId trong DB phải là Guid.Empty (hoặc null nếu bạn ánh xạ)
            // Đảm bảo bạn lưu Guid.Empty vào DB khi tạo Global setting với ScopeId là null/rỗng
            settingsQuery = settingsQuery.Where(c => c.ScopeId == Guid.Empty);
        }
        else if (requestedScopeType != null && parsedScopeId.HasValue) // non-Global scope với ScopeId được cung cấp
        {
            // Đối với non-Global, ScopeId trong DB phải khớp với parsedScopeId và không được là Guid.Empty
            settingsQuery = settingsQuery.Where(c => c.ScopeId == parsedScopeId.Value && c.ScopeId != Guid.Empty);
        }
        // Nếu requestedScopeType là non-Global nhưng parsedScopeId không có giá trị (null hoặc rỗng sau khi parse)
        // thì không cần thêm điều kiện lọc ScopeId, hoặc có thể thêm điều kiện cho trường hợp không khớp (tùy nghiệp vụ)
        // Hiện tại, validator sẽ bắt lỗi này ở CreateSetting, còn ở GetSettings,
        // nếu ScopeId rỗng cho non-Global, chúng ta chỉ không lọc theo ScopeId đó.


        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var lowerSearchTerm = query.SearchTerm.ToLower();
            settingsQuery = settingsQuery.Where(c =>
                (c.AttributeDefinition != null && (c.AttributeDefinition.Key.ToLower().Contains(lowerSearchTerm) ||
                                                   c.AttributeDefinition.DisplayName.ToLower()
                                                       .Contains(lowerSearchTerm))) ||
                c.Value.ToLower().Contains(lowerSearchTerm));
        }

        var totalCount = await settingsQuery.CountAsync(cancellationToken);

        var settings = await settingsQuery
            .OrderBy(c => c.CreatedAt) // Có thể thêm OrderBy động như GetListAttributeDefinitionQueryHandler
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var configDtos = settings.Select(c => new SettingDto
        {
            Id = c.Id,
            AttributeId = c.AttributeId,
            AttributeKey = c.AttributeDefinition?.Key ?? "N/A",
            AttributeDisplayName = c.AttributeDefinition?.DisplayName ?? "N/A",
            DataType = c.AttributeDefinition?.DataType != null ? c.AttributeDefinition.DataType.ToString() : "N/A",
            ScopeType = c.ScopeType != null ? c.ScopeType.ToString() : "N/A",
            ScopeId = c.ScopeId, // Sử dụng Guid ScopeId trực tiếp từ entity
            Value = c.Value,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        var response = new GetSettingsResponse
        {
            Items = configDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return response;
    }
}
