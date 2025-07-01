using MediatR;
using Zentry.Modules.ConfigurationManagement.Dtos;

namespace Zentry.Modules.ConfigurationManagement.Features.ViewConfigurations;

public record GetConfigurationsQuery : IRequest<List<ConfigurationDto>>
{
}