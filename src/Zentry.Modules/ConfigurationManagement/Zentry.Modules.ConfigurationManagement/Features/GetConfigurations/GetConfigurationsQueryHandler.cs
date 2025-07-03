using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Features.GetConfigurations;

public class
    GetConfigurationsQueryHandler(
        ConfigurationDbContext dbContext)
    : IQueryHandler<GetConfigurationsQuery, GetConfigurationsResponse>
{
    public async Task<GetConfigurationsResponse> Handle(GetConfigurationsQuery query,
        CancellationToken cancellationToken)
    {
        IQueryable<Configuration> configurationsQuery = dbContext.Configurations
            .Include(c => c.AttributeDefinition);

        if (query.AttributeId.HasValue)
        {
            configurationsQuery = configurationsQuery.Where(c => c.AttributeId == query.AttributeId.Value);
        }

        // Chuyển đổi string ScopeTypeString từ query sang Smart Enum
        if (!string.IsNullOrWhiteSpace(query.ScopeTypeString))
        {
            ScopeType scopeType;
            try
            {
                scopeType = ScopeType.FromName(query.ScopeTypeString);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessLogicException($"Invalid ScopeType provided: {ex.Message}");
            }

            configurationsQuery = configurationsQuery.Where(c => c.ScopeType == scopeType);
        }

        if (query.ScopeId.HasValue)
        {
            configurationsQuery = configurationsQuery.Where(c => c.ScopeId == query.ScopeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string lowerSearchTerm = query.SearchTerm.ToLower();
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

        return new GetConfigurationsResponse
        {
            Items = configDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
