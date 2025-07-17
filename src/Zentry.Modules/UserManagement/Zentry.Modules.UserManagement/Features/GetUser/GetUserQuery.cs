using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.GetUser;

// Query để lấy thông tin chi tiết người dùng
public class GetUserQuery(Guid userId) : IQuery<GetUserResponse>
{
    public Guid UserId { get; init; } = userId;
}

public class GetUserResponse
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Ví dụ: Active, Inactive
    public DateTime CreatedAt { get; set; }
}
