using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Enums;
using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Persistence.Entities;

namespace Zentry.Modules.UserManagement.Persistence.Repositories;

public class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public async Task Add(Account account, User user)
    {
        await dbContext.Accounts.AddAsync(account);
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await dbContext.Accounts.AnyAsync(a => a.Email == email);
    }


    public async Task<Account?> GetAccountById(Guid accountId)
    {
        return await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
    }

    public async Task<Account?> GetAccountByUserId(Guid userId)
    {
        return await dbContext.Accounts
            .Join(dbContext.Users,
                account => account.Id,
                user => user.AccountId,
                (account, user) => new { Account = account, User = user })
            .Where(joined => joined.User.Id == userId)
            .Select(joined => joined.Account)
            .FirstOrDefaultAsync();
    }

    // Phương thức mới: Cập nhật Account
    public async Task UpdateAccount(Account account)
    {
        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();
    }

    public async Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        string? role,
        string? status)
    {
        var query = from u in dbContext.Users
            join a in dbContext.Accounts on u.AccountId equals a.Id
            select new { User = u, Account = a };

        // Áp dụng lọc
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(x =>
                x.Account.Email.Contains(lowerSearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                x.User.FullName.Contains(lowerSearchTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(x => x.Account.Role == role);

        // ✅ SỬA: Filter cho Smart Enum
        if (!string.IsNullOrWhiteSpace(status))
            try
            {
                var statusEnum = AccountStatus.FromName(status);
                query = query.Where(x => x.Account.Status == statusEnum);
            }
            catch (InvalidOperationException)
            {
            }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(x => x.Account.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItemDto
            {
                UserId = x.User.Id,
                Email = x.Account.Email,
                FullName = x.User.FullName,
                Role = x.Account.Role,
                Status = x.Account.Status.ToString(),
                CreatedAt = x.Account.CreatedAt
            })
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task SoftDeleteUserAsync(Guid userId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            // Xử lý trường hợp người dùng không tồn tại
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == user.AccountId);
        if (account == null)
            // Xử lý trường hợp không tìm thấy tài khoản liên quan
            throw new InvalidOperationException($"Associated account for user ID '{userId}' not found.");

        // Cập nhật trạng thái của tài khoản thành "Deleted" hoặc "Inactive"
        // Bạn nên định nghĩa các hằng số hoặc enum cho các trạng thái này
        account.UpdateStatus(AccountStatus.Inactive); // Giả sử "Inactive" là trạng thái soft delete

        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken: cancellationToken);
    }


    public Task AddAsync(User entity, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use Add(Account, User) for creating new users with accounts.");
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        await SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(User entity, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use Add(Account, User) for creating new users with accounts.");
    }


    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
