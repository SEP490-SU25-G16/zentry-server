using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;

namespace Zentry.SharedKernel.Contracts.User;

public record GetUsersByRoleIntegrationQuery(Role Role) : IQuery<GetUsersByRoleIntegrationResponse>;

public record GetUsersByRoleIntegrationResponse(List<Guid> UserIds);