namespace Zentry.Modules.UserManagement.Features.CreateUser;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Mật khẩu thô từ người dùng
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "User"; // Giá trị mặc định hoặc cần validation
}
