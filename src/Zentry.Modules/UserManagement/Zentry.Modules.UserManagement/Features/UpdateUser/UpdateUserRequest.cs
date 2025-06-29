namespace Zentry.Modules.UserManagement.Features.UpdateUser;

public class UpdateUserRequest
{
    // Các trường mà người dùng có thể cập nhật
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; } // Nếu cho phép cập nhật vai trò qua API này
}
