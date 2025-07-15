using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Features.GetConfigurations;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.ConfigurationManagement.Services;

public class ConfigurationService(IMediator mediator, ILogger<ConfigurationService> logger) : IConfigurationService
{
    public async Task<string?> GetConfigurationValueAsync(string attributeKey, string scopeType, Guid scopeId)
    {
        // Chuyển đổi GetConfigurationRequest thành GetConfigurationsQuery nội bộ
        var query = new GetConfigurationsQuery
        {
            SearchTerm = attributeKey,
            ScopeTypeString = scopeType,
            ScopeId = scopeId,
            PageNumber = 1,
            PageSize = 1
        };

        try
        {
            var response = await mediator.Send(query);
            // Lọc chính xác theo attributeKey vì SearchTerm có thể trả về nhiều hơn
            var configDto = response.Items.FirstOrDefault(c =>
                c.AttributeKey.Equals(attributeKey, StringComparison.OrdinalIgnoreCase) &&
                c.ScopeType.Equals(scopeType, StringComparison.OrdinalIgnoreCase) &&
                c.ScopeId == scopeId
            );

            return configDto?.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve configuration value for Key: {AttributeKey}, ScopeType: {ScopeType}, ScopeId: {ScopeId}",
                attributeKey, scopeType, scopeId);
            return null;
        }
    }

    public async Task<ConfigLookupResponseDto> GetConfigurationsAsync(ConfigLookupRequestDto requestDto)
    {
        var query = new GetConfigurationsQuery
        {
            // AttributeId = request.AttributeId, // GetConfigurationRequest không có AttributeId
            SearchTerm = requestDto.Key, // Sử dụng Key trong contract map với SearchTerm trong query
            ScopeTypeString = requestDto.ScopeType,
            ScopeId = requestDto.ScopeId,
            PageNumber =
                1, // Nếu bạn muốn hỗ trợ phân trang qua contract, thêm PageNumber/PageSize vào GetConfigurationRequest
            PageSize = 1 // và map ở đây
        };

        try
        {
            var internalResponse = await mediator.Send(query);

            // Chuyển đổi từ GetConfigurationsResponse nội bộ (ConfigurationManagement.Features.GetConfigurations)
            // sang GetConfigurationsResponse (SharedKernel.Contracts)
            var contractResponse = new ConfigLookupResponseDto
            {
                Items = internalResponse.Items.Select(item => new ConfigLookupDto
                {
                    Id = item.Id,
                    AttributeId = item.AttributeId,
                    AttributeKey = item.AttributeKey,
                    AttributeDisplayName = item.AttributeDisplayName,
                    DataType = item.DataType,
                    ScopeType = item.ScopeType,
                    ScopeId = item.ScopeId,
                    Value = item.Value,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).ToList(),
                TotalCount = internalResponse.TotalCount,
                PageNumber = internalResponse.PageNumber,
                PageSize = internalResponse.PageSize
            };

            return contractResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve configurations with contract request: {@Request}", requestDto);
            throw;
        }
    }
}
