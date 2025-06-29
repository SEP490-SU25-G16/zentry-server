using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Persistence.Entities;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services; // Thêm using này để có UserDbContext

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
}
