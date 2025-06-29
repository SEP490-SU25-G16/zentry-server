using MediatR;
using Zentry.Modules.ConfigurationManagement.Application.Dtos;

namespace Zentry.Modules.ConfigurationManagement.Application.Features.ViewConfigurations;

public record GetConfigurationsQuery : IRequest<List<ConfigurationDto>>
{
}