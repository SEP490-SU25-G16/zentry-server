namespace Zentry.Modules.UserManagement.Persistence.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Default role

    // Columns for Password Reset Token
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiryTime { get; set; }
}
