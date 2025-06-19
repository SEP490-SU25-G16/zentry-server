using MediatR;
using Zentry.Modules.Configuration.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.Configuration.Application.Features.ViewConfigurations;

public record GetConfigurationsQuery : IQuery<List<Domain.Entities.Configuration>>, IRequest<List<ConfigurationDto>>;
