using MediatR;
using Zentry.Modules.ConfigurationManagement.Application.Dtos;
using Zentry.Modules.ConfigurationManagement.Domain.Entities;

namespace Zentry.Modules.ConfigurationManagement.Application.Features.ViewConfigurations;
public record GetConfigurationsQuery : IRequest<List<ConfigurationDto>>
{

}
