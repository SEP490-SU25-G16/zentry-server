using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.UserManagement.Features.GetUsers;

public class UserListItemDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Role? Role { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
