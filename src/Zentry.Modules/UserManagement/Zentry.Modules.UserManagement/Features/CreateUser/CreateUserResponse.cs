using Zentry.Modules.UserManagement.Persistence.Enums;

namespace Zentry.Modules.UserManagement.Features.CreateUser;

public class CreateUserResponse
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}