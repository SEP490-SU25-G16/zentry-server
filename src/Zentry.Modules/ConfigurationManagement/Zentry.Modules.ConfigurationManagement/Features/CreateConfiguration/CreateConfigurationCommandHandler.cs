using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateConfiguration;

public class
    CreateConfigurationCommandHandler(
        IAttributeService attributeService,
        ConfigurationDbContext dbContext,
        IRedisService redisService,
        ILogger<CreateConfigurationCommandHandler> logger)
    : ICommandHandler<CreateConfigurationCommand, CreateConfigurationResponse>
{
    public async Task<CreateConfigurationResponse> Handle(CreateConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            AttributeDefinition attributeDefinition;
            List<Option> createdOrUpdatedOptions = [];

            // 1. Xử lý AttributeDefinition
            if (command.AttributeDefinitionDetails != null)
            {
                // Chuyển đổi string DataType và ScopeType từ command/DTO sang Smart Enum
                DataType attributeDefinitionDataType;
                ScopeType attributeDefinitionScopeType;
                try
                {
                    attributeDefinitionDataType = DataType.FromName(command.AttributeDefinitionDetails.DataType);
                    attributeDefinitionScopeType = ScopeType.FromName(command.AttributeDefinitionDetails.ScopeType);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid DataType or ScopeType provided for Attribute Definition: {Message}",
                        ex.Message);
                    throw new BusinessLogicException(
                        $"Invalid DataType or ScopeType provided for Attribute Definition: {ex.Message}");
                }

                var existingAttributeDefinition = await dbContext.AttributeDefinitions
                    .FirstOrDefaultAsync(ad => ad.Key == command.AttributeDefinitionDetails.Key, cancellationToken);

                if (existingAttributeDefinition != null)
                {
                    if (command.AttributeDefinitionDetails.Id.HasValue &&
                        existingAttributeDefinition.Id == command.AttributeDefinitionDetails.Id.Value)
                    {
                        existingAttributeDefinition.Update(
                            command.AttributeDefinitionDetails.DisplayName,
                            command.AttributeDefinitionDetails.Description,
                            attributeDefinitionDataType,
                            attributeDefinitionScopeType,
                            command.AttributeDefinitionDetails.Unit
                        );
                        dbContext.AttributeDefinitions.Update(existingAttributeDefinition);
                        attributeDefinition = existingAttributeDefinition;
                    }
                    else
                    {
                        logger.LogWarning("Attribute Definition with Key '{Key}' already exists with a different ID.",
                            command.AttributeDefinitionDetails.Key);
                        throw new BusinessLogicException(
                            $"Attribute Definition with Key '{command.AttributeDefinitionDetails.Key}' already exists with a different ID.");
                    }
                }
                else
                {
                    attributeDefinition = AttributeDefinition.Create(
                        command.AttributeDefinitionDetails.Key,
                        command.AttributeDefinitionDetails.DisplayName,
                        command.AttributeDefinitionDetails.Description,
                        attributeDefinitionDataType,
                        attributeDefinitionScopeType,
                        command.AttributeDefinitionDetails.Unit
                    );
                    await dbContext.AttributeDefinitions.AddAsync(attributeDefinition, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                // 2. Xử lý Options nếu DataType là Selection
                if (attributeDefinition.DataType == DataType.Selection)
                {
                    if (command.AttributeDefinitionDetails.Options != null &&
                        command.AttributeDefinitionDetails.Options.Count != 0)
                    {
                        // Xóa tất cả options cũ
                        var oldOptions = await dbContext.Options
                            .Where(o => o.AttributeId == attributeDefinition.Id)
                            .ToListAsync(cancellationToken);
                        if (oldOptions.Count != 0)
                        {
                            dbContext.Options.RemoveRange(oldOptions);
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }


                        // Tạo options mới
                        foreach (var optionDto in command.AttributeDefinitionDetails.Options)
                        {
                            var newOption = Option.Create(
                                attributeDefinition.Id,
                                optionDto.Value,
                                optionDto.DisplayLabel,
                                optionDto.SortOrder
                            );
                            createdOrUpdatedOptions.Add(newOption);
                            await dbContext.Options.AddAsync(newOption, cancellationToken);
                        }

                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Attribute Definition with DataType 'Selection' must have options provided for Key '{Key}'.",
                            attributeDefinition.Key);
                        throw new BusinessLogicException(
                            "Attribute Definition with DataType 'Selection' must have options provided.");
                    }
                }
            }
            else
            {
                logger.LogWarning(
                    "AttributeDefinitionDetails is required to create or update an Attribute Definition.");
                throw new BusinessLogicException(
                    "AttributeDefinitionDetails is required to create or update an Attribute Definition.");
            }

            // 3. Chuyển đổi string ScopeType của Configuration từ command/DTO sang Smart Enum
            ScopeType configurationScopeType;
            try
            {
                configurationScopeType = ScopeType.FromName(command.Configuration.ScopeType);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid ScopeType provided for Configuration: {Message}", ex.Message);
                throw new BusinessLogicException($"Invalid ScopeType provided for Configuration: {ex.Message}");
            }

            // 4. Validate Value của Configuration dựa trên DataType của AttributeDefinition
            // QUAN TRỌNG: Phải validate sau khi đã lưu Options (nếu có) vào database
            if (!await attributeService.IsValueValidForAttribute(attributeDefinition.Id, command.Configuration.Value))
            {
                logger.LogWarning("Provided value '{Value}' is not valid for Attribute '{Key}' (DataType: {DataType}).",
                    command.Configuration.Value, attributeDefinition.Key, attributeDefinition.DataType);
                throw new BusinessLogicException(
                    $"Provided value '{command.Configuration.Value}' is not valid for Attribute '{attributeDefinition.DisplayName}' (DataType: {attributeDefinition.DataType}).");
            }


            // 5. Kiểm tra xem cấu hình cho AttributeId, ScopeType, ScopeId đã tồn tại chưa
            var existingConfiguration = await dbContext.Configurations
                .FirstOrDefaultAsync(c => c.AttributeId == attributeDefinition.Id &&
                                          c.ScopeType == configurationScopeType &&
                                          c.ScopeId == command.Configuration.ScopeId, cancellationToken);

            if (existingConfiguration != null)
            {
                logger.LogWarning(
                    "Configuration for Attribute '{Key}' with Scope '{ScopeType}' and ScopeId '{ScopeId}' already exists.",
                    attributeDefinition.Key, configurationScopeType, command.Configuration.ScopeId);
                throw new BusinessLogicException(
                    $"Configuration for Attribute '{attributeDefinition.Key}' with Scope '{configurationScopeType}' and ScopeId '{command.Configuration.ScopeId}' already exists.");
            }

            // 6. Tạo Configuration entity mới
            var newConfiguration = Configuration.Create(
                attributeDefinition.Id,
                configurationScopeType,
                command.Configuration.ScopeId,
                command.Configuration.Value
            );

            // 7. Thêm Configuration vào DbContext
            await dbContext.Configurations.AddAsync(newConfiguration, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            // 8. Hoàn thành giao dịch
            await transaction.CommitAsync(cancellationToken);

            // --- BẮT ĐẦU PHẦN XÓA CACHE SAU KHI DB ĐÃ CÓ DATA MỚI ---
            await InvalidateConfigurationCache(
                attributeDefinition.Id,
                newConfiguration.ScopeType,
                newConfiguration.ScopeId);
            // --- KẾT THÚC PHẦN XÓA CACHE ---

            // 9. Trả về Response DTO
            var optionDtos = createdOrUpdatedOptions.Select(o => new OptionDto
            {
                Id = o.Id,
                Value = o.Value,
                DisplayLabel = o.DisplayLabel,
                SortOrder = o.SortOrder
            }).ToList();

            logger.LogInformation(
                "Configuration {ConfigurationId} created successfully for Attribute '{AttributeKey}' with Scope '{ScopeType}' and ScopeId '{ScopeId}'.",
                newConfiguration.Id, attributeDefinition.Key, newConfiguration.ScopeType, newConfiguration.ScopeId);

            return new CreateConfigurationResponse
            {
                ConfigurationId = newConfiguration.Id,
                AttributeId = attributeDefinition.Id,
                AttributeKey = attributeDefinition.Key,
                AttributeDisplayName = attributeDefinition.DisplayName,
                DataType = attributeDefinition.DataType,
                AttributeDefinitionScopeType = attributeDefinition.ScopeType,
                Unit = attributeDefinition.Unit,
                Options = optionDtos.Any() ? optionDtos : null,
                ConfigurationScopeType = newConfiguration.ScopeType,
                ScopeId = newConfiguration.ScopeId,
                Value = newConfiguration.Value,
                CreatedAt = newConfiguration.CreatedAt,
                UpdatedAt = newConfiguration.UpdatedAt
            };
        }
        catch (BusinessLogicException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw; // Re-throw business logic exceptions
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex,
                "An unexpected error occurred while creating configuration for attribute '{AttributeKey}' and scope '{ScopeType}' '{ScopeId}'.",
                command.AttributeDefinitionDetails?.Key, command.Configuration?.ScopeType,
                command.Configuration?.ScopeId);
            throw; // Re-throw other exceptions
        }
    }

    /// <summary>
    ///     Xóa các cache keys liên quan đến cấu hình vừa được tạo/cập nhật.
    /// </summary>
    /// <param name="attributeId">ID của AttributeDefinition.</param>
    /// <param name="scopeType">Loại phạm vi (Global, Course, Session).</param>
    /// <param name="scopeId">ID của phạm vi.</param>
    private async Task InvalidateConfigurationCache(Guid attributeId, ScopeType scopeType, Guid scopeId)
    {
        // Các cache keys được tạo trong GetConfigurationsQueryHandler có dạng:
        // "configurations:{query.AttributeId?.ToString() ?? "null"}:{query.ScopeTypeString ?? "null"}:{query.ScopeId?.ToString() ?? "null"}:{query.SearchTerm ?? "null"}:{query.PageNumber}:{query.PageSize}";

        // Để invalidate chính xác, chúng ta cần xóa tất cả các keys có thể được tạo ra
        // từ các truy vấn GetConfigurationsQuery khác nhau nhưng liên quan đến cấu hình này.

        // Vì GetConfigurationsQueryHandler có tham số PageNumber, PageSize và SearchTerm
        // việc xóa chính xác từng key là không thực tế với Redis Keys (nên dùng SCAN nếu số lượng lớn).
        // Tuy nhiên, vì chúng ta chỉ cache khi ScopeType là GLOBAL, COURSE, SESSION VÀ KHÔNG CÓ SearchTerm,
        // chúng ta có thể tập trung vào các keys có dạng đó.

        // Giả định rằng GetConfigurationsQueryHandler chỉ cache các request KHÔNG CÓ SearchTerm và có ScopeType cụ thể.
        // Cache key sẽ là: "configurations:{attributeId}:{scopeType.Name}:{scopeId}:null:{pageNumber}:{pageSize}"

        logger.LogInformation(
            "Attempting to invalidate Redis cache for AttributeId: {AttributeId}, ScopeType: {ScopeType}, ScopeId: {ScopeId}",
            attributeId, scopeType, scopeId);

        // 1. Xóa cache của chính cấu hình vừa thay đổi (nếu nó được truy vấn trực tiếp)
        // Đây là key của một cấu hình CỤ THỂ, nếu có query chỉ định chính xác AttributeId, ScopeType và ScopeId.
        var preciseKey =
            $"configurations:{attributeId}:{scopeType}:{scopeId}:null:1:1"; // Giả định page 1, size 1 nếu có query chính xác

        var removedPreciseKey = await redisService.RemoveAsync(preciseKey);
        if (removedPreciseKey) logger.LogDebug("Removed precise cache key: {Key}", preciseKey);

        // 2. Xóa các cache tổng quát hơn có thể bị ảnh hưởng.
        // Đây là phần khó và có nhiều chiến lược. Dưới đây là một số ví dụ đơn giản:

        // a) Xóa các cache keys cho AttributeId này mà không chỉ định ScopeId (áp dụng cho tất cả ScopeId của cùng ScopeType)
        // Ví dụ: configurations:{attributeId}:{scopeType.Name}:null:null:*:*
        // Do chúng ta không thể dùng wildcard với REMOVE trực tiếp, ta sẽ tạo key "tổng quát" nhất của nhóm này
        // và giả định nó có thể được cache.
        var generalAttrScopeKey = $"configurations:{attributeId}:null:null:null:*:*";
        // Việc xóa key này là không hiệu quả nếu nó không tồn tại, và nếu nó có wildcards.
        // Để làm điều này hiệu quả, bạn cần phải:
        //   - Lưu danh sách các keys đã cache vào một Redis Set/Hash riêng.
        //   - Hoặc sử dụng Redis Streams/PubSub để thông báo thay đổi và các service khác tự purge cache của chúng.
        //   - Hoặc chấp nhận TTL ngắn cho cache.

        // Với cấu trúc cache key bạn có:
        // "configurations:{AttributeId?}:{ScopeType?}:{ScopeId?}:{SearchTerm?}:{PageNumber}:{PageSize}"
        // Để invalidate các kết quả tìm kiếm cho một AttributeId, ScopeType, ScopeId cụ thể:

        // Invalidating configurations by specific AttributeId, ScopeType, and ScopeId
        // This targets queries for a specific configuration
        var keyPatternSpecific = $"configurations:{attributeId}:{scopeType}:{scopeId}:*";
        await DeleteKeysByPattern(keyPatternSpecific);

        // Invalidating configurations by specific AttributeId and ScopeType (for queries without ScopeId specified)
        // This targets queries that might retrieve this config along with others in the same scope type
        var keyPatternByAttrAndScopeType = $"configurations:{attributeId}:{scopeType}:null:*";
        await DeleteKeysByPattern(keyPatternByAttrAndScopeType);

        // Invalidating configurations by specific AttributeId (for queries without ScopeType/ScopeId specified)
        var keyPatternByAttr = $"configurations:{attributeId}:null:null:*";
        await DeleteKeysByPattern(keyPatternByAttr);

        // Invalidating configurations by specific ScopeType (for queries without AttributeId/ScopeId specified)
        var keyPatternByScopeType = $"configurations:null:{scopeType}:*";
        await DeleteKeysByPattern(keyPatternByScopeType);

        // Invalidating global configurations (if this change impacts global configurations, e.g., if scopeType is GLOBAL)
        if (scopeType == ScopeType.GLOBAL)
        {
            var keyPatternGlobal = "configurations:null:GLOBAL:*"; // Only for Global scopeType
            await DeleteKeysByPattern(keyPatternGlobal);
            // Also consider invalidating generic global lists (e.g., configurations:null:null:null:*)
            var keyPatternVeryGeneral = "configurations:null:null:null:*";
            await DeleteKeysByPattern(keyPatternVeryGeneral);
        }

        logger.LogInformation("Finished attempting to invalidate Redis cache for configurations.");
    }

    /// <summary>
    ///     Helper method to delete keys matching a pattern.
    ///     WARNING: Using KEYS * in production with a large dataset can cause performance issues.
    ///     Consider using SCAN command for production environments.
    ///     For small datasets or development, this is acceptable.
    /// </summary>
    /// <param name="redisService">The IRedisService instance.</param>
    /// <param name="pattern">The key pattern (e.g., "prefix:*:suffix").</param>
    private async Task DeleteKeysByPattern(string pattern)
    {
        // This is a simplified approach for demonstration.
        // In a real-world, large-scale application, you'd use IDatabase.Keys(pattern: ...) with SCAN
        // or a more sophisticated cache invalidation strategy (e.g., storing related keys in a Redis Set).
        logger.LogWarning(
            "Attempting to delete Redis keys by pattern '{Pattern}'. This uses KEYS * implicitly and can be inefficient on large datasets.",
            pattern);

        // Note: IRedisService's RemoveAsync takes a single key, not a pattern.
        // To use patterns, you'd typically need to get the actual keys first.
        // As a fallback for IRedisService's current capabilities, if it doesn't expose `Keys()`
        // we'd have to make an assumption about key construction.
        // For accurate pattern matching and deletion within this IRedisService setup,
        // I'd need to add a `GetKeysByPatternAsync` method to IRedisService.

        // For now, I'll rely on the specific key constructions in GetConfigurationsQueryHandler.
        // The most effective keys to target for invalidation are those created WITHOUT SearchTerm and With specific ScopeTypes.
        // Example: configurations:{attributeId}:{scopeType.Name}:{scopeId}:null:1:1
        // We can't use wildcards for PageNumber and PageSize directly with IRedisService.RemoveAsync.
        // The best approach here is to invalidate based on `attributeId`, `scopeType`, `scopeId`,
        // and assume GetConfigurationsQueryHandler only caches results *without* a search term.
        // So, we'll try to remove the most common variations.

        await redisService.RemoveAsync($"{pattern}:null:1:1"); // Assuming a common page/size
        await redisService.RemoveAsync($"{pattern}:null:*:*"); // If you had a mechanism to list them
        // Add more specific key removals if you know the exact patterns
        // E.g., if you know the common page sizes and page numbers used:
        // await redisService.RemoveAsync($"{commonPrefix}:null:1:10");
        // await redisService.RemoveAsync($"{commonPrefix}:null:1:20");
        // ... etc.

        // For a more robust solution, I'd suggest extending IRedisService with a method like:
        // Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
        // Then:
        // var keysToDelete = await redisService.GetKeysByPatternAsync(pattern);
        // foreach (var key in keysToDelete) { await redisService.RemoveAsync(key); }
        logger.LogInformation("Simplified cache invalidation attempt for pattern: {Pattern}", pattern);
    }
}