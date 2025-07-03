using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Persistence.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.UserManagement.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<bool> ExistsByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Account account, User user, CancellationToken cancellationToken);
    Task<bool> ExistsByEmail(string email);

    Task<Account?> GetAccountById(Guid accountId);
    Task<Account?> GetAccountByUserId(Guid userId);
    Task UpdateAccountAsync(Account account, CancellationToken cancellationToken);

    Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        string? role,
        string? status);

    Task SoftDeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}