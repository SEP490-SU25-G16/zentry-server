using MediatR;

namespace Zentry.SharedKernel.Contracts.User;

public record GetUsersByRoleIntegrationQuery(string Role) : IRequest<GetUsersByRoleIntegrationResponse>;

public record GetUsersByRoleIntegrationResponse(List<Guid> UserIds);