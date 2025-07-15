namespace Zentry.SharedKernel.Contracts.User;

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