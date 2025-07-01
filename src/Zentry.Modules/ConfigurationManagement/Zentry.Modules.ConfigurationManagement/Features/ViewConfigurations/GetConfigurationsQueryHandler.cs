using MediatR;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Dtos;

namespace Zentry.Modules.ConfigurationManagement.Features.ViewConfigurations;

public class GetConfigurationsQueryHandler(IConfigurationRepository repository)
    : IRequestHandler<GetConfigurationsQuery, List<ConfigurationDto>>
{
    public Task<List<ConfigurationDto>> Handle(GetConfigurationsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}