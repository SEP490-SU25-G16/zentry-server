using MediatR;
using Zentry.Modules.ConfigurationManagement.Application.Abstractions;
using Zentry.Modules.ConfigurationManagement.Application.Dtos;

namespace Zentry.Modules.ConfigurationManagement.Application.Features.ViewConfigurations;

public class GetConfigurationsQueryHandler(IConfigurationRepository repository)
    : IRequestHandler<GetConfigurationsQuery, List<ConfigurationDto>>
{
    public Task<List<ConfigurationDto>> Handle(GetConfigurationsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}