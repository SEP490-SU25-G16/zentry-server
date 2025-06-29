namespace Zentry.Modules.UserManagement.Features.GetUsers;

// DTO đại diện cho một người dùng trong danh sách
public class UserListItemDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}