using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public class SignInCommand : ICommand<SignInResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // ✅ Thêm DeviceToken để validate device
    public string DeviceToken { get; set; } = string.Empty;
}

public class SignInResponse
{
    public string SessionKey { get; set; } = string.Empty; // ✅ Thay JWT bằng SessionKey
    public UserInfo UserInfo { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}