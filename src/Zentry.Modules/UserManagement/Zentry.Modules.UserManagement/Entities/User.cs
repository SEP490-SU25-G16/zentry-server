using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.UserManagement.Persistence.Entities;

public class User : AggregateRoot<Guid>
{
    private User() : base(Guid.Empty)
    {
    }

    private User(Guid id, Guid accountId, string fullName, string? phoneNumber)
        : base(id)
    {
        AccountId = accountId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid AccountId { get; private set; }
    public string FullName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static User Create(Guid accountId, string fullName, string? phoneNumber)
    {
        return new User(Guid.NewGuid(), accountId, fullName, phoneNumber);
    }

    public void UpdateUser(string? fullName = null, string? phoneNumber = null)
    {
        if (!string.IsNullOrWhiteSpace(fullName)) FullName = fullName;
        if (!string.IsNullOrWhiteSpace(phoneNumber)) PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }
}