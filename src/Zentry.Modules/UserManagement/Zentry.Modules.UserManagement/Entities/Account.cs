using Zentry.SharedKernel.Domain;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.UserManagement.Entities;

public class Account : AggregateRoot<Guid>
{
    private Account() : base(Guid.Empty)
    {
    }

    private Account(Guid id, string email, string passwordHash, string passwordSalt, Role role) // Thay string bằng Role
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        Role = role; // Gán Role Smart Enum
        CreatedAt = DateTime.UtcNow;
        Status = AccountStatus.Active;
    }

    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string PasswordSalt { get; private set; }
    public Role Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AccountStatus Status { get; private set; }
    public string? ResetToken { get; private set; }
    public DateTime? ResetTokenExpiryTime { get; private set; }


    public static Account
        Create(string email, string passwordHash, string passwordSalt, Role role) // Thay string bằng Role
    {
        return new Account(Guid.NewGuid(), email, passwordHash, passwordSalt, role);
    }

    public void UpdateAccount(string? email = null, Role? role = null) // Thay string bằng Role?
    {
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        if (role != null) Role = role; // Gán Role Smart Enum nếu không null
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(AccountStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNewPassword(string newPasswordHash, string newPasswordSalt)
    {
        PasswordHash = newPasswordHash;
        PasswordSalt = newPasswordSalt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetResetToken(string token, DateTime expiryTime)
    {
        ResetToken = token;
        ResetTokenExpiryTime = expiryTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearResetToken()
    {
        ResetToken = null;
        ResetTokenExpiryTime = null;
        UpdatedAt = DateTime.UtcNow;
    }
}