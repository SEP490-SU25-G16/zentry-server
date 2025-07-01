using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Persistence.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.UserManagement.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
    Task Add(Account account, User user);
    Task<bool> ExistsByEmail(string email);

    Task<Account?> GetAccountById(Guid accountId);
    Task<Account?> GetAccountByUserId(Guid userId);
    Task UpdateAccount(Account account);
    Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        string? role,
        string? status);

    Task SoftDeleteUserAsync(Guid userId);
}
