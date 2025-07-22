using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.User;

public record GetUserByIdAndRoleIntegrationQuery(string Role, Guid UserId)
    : IQuery<GetUserByIdAndRoleIntegrationResponse>;

public record GetUserByIdAndRoleIntegrationResponse(
    Guid UserId,
    Guid AccountId,
    string Email,
    string FullName,
    string? PhoneNumber,
    string Role,
    string Status,
    DateTime CreatedAt
);
