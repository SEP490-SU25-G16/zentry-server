using MediatR;
using Zentry.Modules.Configuration.Application.Abstractions;
using Zentry.Modules.Configuration.Application.Dtos;

namespace Zentry.Modules.Configuration.Application.Features.ViewConfigurations;

public class GetConfigurationsQueryHandler(IConfigurationRepository repository)
    : IRequestHandler<GetConfigurationsQuery, List<ConfigurationDto>>
{
    public async Task<List<ConfigurationDto>> Handle(GetConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        var configs = await repository.GetAllAsync(cancellationToken);
        return configs.Select(c => new ConfigurationDto(
            Guid.NewGuid(),
            c.Key,
            c.Value,
            c.Description,
            c.CreatedAt,
            c.UpdatedAt
        )).ToList();
    }
}