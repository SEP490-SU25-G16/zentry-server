using MediatR;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.User;

public record GetUsersByRoleIntegrationQuery(string Role) : IQuery<GetUsersByRoleIntegrationResponse>;

public record GetUsersByRoleIntegrationResponse(List<Guid> UserIds);
