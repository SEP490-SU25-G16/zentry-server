using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.User;

public record GetUserByIdAndRoleIntegrationQuery(string Role, Guid UserId)
    : IQuery<GetUserByIdAndRoleIntegrationResponse>;