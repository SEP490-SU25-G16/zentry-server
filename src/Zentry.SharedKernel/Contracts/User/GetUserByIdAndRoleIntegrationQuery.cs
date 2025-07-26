using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Enums.User;

// Đảm bảo đã có using này

namespace Zentry.SharedKernel.Contracts.User;

public record GetUserByIdAndRoleIntegrationQuery(Role Role, Guid UserId)
    : IQuery<GetUserByIdAndRoleIntegrationResponse>;

public record GetUserByIdAndRoleIntegrationResponse(
    Guid UserId,
    Guid AccountId,
    string Email,
    string FullName,
    string? PhoneNumber,
    Role Role,
    string Status,
    DateTime CreatedAt
);