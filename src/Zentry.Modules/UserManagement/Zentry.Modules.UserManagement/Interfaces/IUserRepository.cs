using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Persistence.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.UserManagement.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
    Task Add(Account account, User user);
    Task<bool> ExistsByEmail(string email);

    Task<User?> GetUserById(Guid userId);
    Task<Account?> GetAccountById(Guid accountId); // Lấy Account theo ID của nó

    // Phương thức mới: Lấy Account theo User ID (vì User có AccountId)
    Task<Account?> GetAccountByUserId(Guid userId); // Phương thức này sẽ thuận tiện hơn

    Task UpdateUser(User user);
    Task UpdateAccount(Account account);

    Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        string? role,
        string? status);

    Task SoftDeleteUserAsync(Guid userId);
}
