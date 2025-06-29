using Zentry.Modules.UserManagement.Persistence.Entities;

namespace Zentry.Modules.UserManagement.Interfaces;

public interface IUserRepository
{
    Task Add(Account account, User user);
    Task<bool> ExistsByEmail(string email);

    // Phương thức mới: Tìm User theo ID
    Task<User?> GetUserById(Guid userId);

    // Phương thức mới: Tìm Account theo ID (của Account)
    Task<Account?> GetAccountById(Guid accountId);

    // Phương thức mới: Cập nhật User
    Task UpdateUser(User user);

    // Phương thức mới: Cập nhật Account
    Task UpdateAccount(Account account);

    // Có thể thêm một phương thức SaveChanges tổng quát nếu bạn không muốn SaveChanges trong mỗi Add/Update
    // Task SaveChangesAsync();
}
