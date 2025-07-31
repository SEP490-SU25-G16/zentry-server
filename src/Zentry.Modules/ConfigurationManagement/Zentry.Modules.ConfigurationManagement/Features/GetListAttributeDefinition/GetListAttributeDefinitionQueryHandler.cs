using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.SharedKernel.Constants.Configuration;

namespace Zentry.Modules.ConfigurationManagement.Features.GetListAttributeDefinition;

public class GetListAttributeDefinitionQueryHandler(
    ConfigurationDbContext dbContext)
    : IRequestHandler<GetListAttributeDefinitionQuery, GetListAttributeDefinitionResponse>
{
    public async Task<GetListAttributeDefinitionResponse> Handle(GetListAttributeDefinitionQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = dbContext.AttributeDefinitions.AsQueryable();

        // Thêm .Include() để tải Options cùng với AttributeDefinition
        queryable = queryable.Include(ad => ad.Options);

        // ... (Các bộ lọc và sắp xếp khác của bạn) ...
        if (!string.IsNullOrWhiteSpace(query.Key)) queryable = queryable.Where(ad => ad.Key.Contains(query.Key));

        if (!string.IsNullOrWhiteSpace(query.DisplayName))
            queryable = queryable.Where(ad => ad.DisplayName.Contains(query.DisplayName));

        if (!string.IsNullOrWhiteSpace(query.DataType))
            queryable = queryable.Where(ad => ad.DataType == DataType.FromName(query.DataType));

        if (!string.IsNullOrWhiteSpace(query.ScopeType))
            queryable = queryable.Where(ad =>
                ad.AllowedScopeTypes.Any(st => st == ScopeType.FromName(query.ScopeType)));

        if (!string.IsNullOrWhiteSpace(query.OrderBy))
        {
            var parts = query.OrderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var propertyName = parts[0].ToLower();
            var direction = parts.Length > 1 ? parts[1].ToLower() : "asc";

            Expression<Func<AttributeDefinition, object>> orderByExpression = propertyName switch
            {
                "key" => ad => ad.Key,
                "displayname" => ad => ad.DisplayName,
                "datatype" => ad => ad.DataType.ToString(),
                "createdat" => ad => ad.CreatedAt,
                "updatedat" => ad => ad.UpdatedAt,
                _ => ad => ad.CreatedAt
            };

            queryable = direction == "desc"
                ? queryable.OrderByDescending(orderByExpression)
                : queryable.OrderBy(orderByExpression);
        }
        else
        {
            queryable = queryable.OrderByDescending(ad => ad.CreatedAt);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ad =>
                new AttributeDefinitionListItemDto
                {
                    AttributeId = ad.Id,
                    Key = ad.Key,
                    DisplayName = ad.DisplayName,
                    Description = ad.Description,
                    DataType = ad.DataType,
                    AllowedScopeTypes = ad.AllowedScopeTypes,
                    Unit = ad.Unit,
                    DefaultValue = ad.DefaultValue,
                    IsDeletable = ad.IsDeletable,
                    CreatedAt = ad.CreatedAt,
                    UpdatedAt = ad.UpdatedAt,
                    Options = ad.Options.Select(opt => new OptionDto
                    {
                        Id = opt.Id,
                        Value = opt.Value,
                        DisplayLabel = opt.DisplayLabel,
                        SortOrder = opt.SortOrder
                    }).ToList()
                })
            .ToListAsync(cancellationToken);

        return new GetListAttributeDefinitionResponse
        {
            AttributeDefinitions = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}