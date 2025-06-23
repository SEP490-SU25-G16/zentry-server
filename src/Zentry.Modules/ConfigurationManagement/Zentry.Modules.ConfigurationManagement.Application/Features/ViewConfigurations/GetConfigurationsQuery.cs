using MediatR;
using Zentry.Modules.ConfigurationManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ConfigurationManagement.Application.Features.ViewConfigurations;

public record GetConfigurationsQuery : IQuery<List<Domain.Entities.Configuration>>, IRequest<List<ConfigurationDto>>;