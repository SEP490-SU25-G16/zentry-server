using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Enums;
using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Persistence.Entities;
using Zentry.Modules.UserManagement.Persistence.Enums;

namespace Zentry.Modules.UserManagement.Persistence.Repositories;

public class UserRepository(UserDbContext dbContext) : IUserRepository
{
    // Đã sửa từ DbContext sang UserDbContext

    // Constructor đã sửa

    public async Task Add(Account account, User user)
    {
        await dbContext.Accounts.AddAsync(account); // Sử dụng DbSet trực tiếp
        await dbContext.Users.AddAsync(user); // Sử dụng DbSet trực tiếp
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await dbContext.Accounts.AnyAsync(a => a.Email == email);
    }

    // Phương thức mới: Tìm User theo ID
    public async Task<User?> GetUserById(Guid userId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    // Phương thức mới: Tìm Account theo ID (của Account)

    public async Task<Account?> GetAccountById(Guid accountId)
    {
        return await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
    }

    // Phương thức mới: Lấy Account theo User ID
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

    // Phương thức mới: Cập nhật User
    public async Task UpdateUser(User user)
    {
        dbContext.Users.Update(user); // Đánh dấu entity là Modified
        await dbContext.SaveChangesAsync();
    }

    // Phương thức mới: Cập nhật Account
    public async Task UpdateAccount(Account account)
    {
        dbContext.Accounts.Update(account); // Đánh dấu entity là Modified
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
            query = query.Where(x => x.Account.Email.ToLower().Contains(lowerSearchTerm) ||
                                     x.User.FullName.ToLower().Contains(lowerSearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(x => x.Account.Role == role);

        // ✅ SỬA: Filter cho Smart Enum
        if (!string.IsNullOrWhiteSpace(status))
        {
            try
            {
                var statusEnum = AccountStatus.FromName(status);
                query = query.Where(x => x.Account.Status == statusEnum);
            }
            catch (InvalidOperationException)
            {
                // Ignore invalid status filter
            }
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
                Status = x.Account.Status.ToString(), // Smart Enum tự động convert sang string
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

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(User entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Update(User entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(User entity)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
