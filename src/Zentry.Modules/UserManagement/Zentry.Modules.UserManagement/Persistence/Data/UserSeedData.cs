using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Enums;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services; // For IPasswordHasher

namespace Zentry.Modules.UserManagement.Persistence.Data;

public static class UserSeedData
{
    // REMOVE: private static readonly IPasswordHasher PasswordHasher = new PasswordHasher();

    // UPDATED: Danh sách các vai trò có sẵn trong hệ thống
    private static readonly string[] SystemRoles = { "Admin", "Manager", "Lecturer", "Student" };

    public static async Task SeedAsync(UserDbContext context, IPasswordHasher passwordHasher, ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Starting User Management seed data...");

            // Kiểm tra nếu đã có data thì không seed nữa
            if (await context.Accounts.AnyAsync())
            {
                logger?.LogInformation("User Management data already exists. Skipping seed.");
                return;
            }

            // 1. Seed Admin Accounts (Real data)
            await SeedAdminAccounts(context, passwordHasher, logger);

            // 2. Seed Regular Users (Lecturer and Student) với Bogus (Fake data)
            await SeedRegularUsers(context, passwordHasher, logger);

            // 3. Seed Test Accounts cho development (Nếu bạn vẫn cần)
            await SeedTestAccounts(context, passwordHasher, logger); // Giữ lại nếu cần các tài khoản test cụ thể

            await context.SaveChangesAsync();
            logger?.LogInformation("User Management seed data completed successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding User Management data");
            throw;
        }
    }

    private static async Task SeedAdminAccounts(UserDbContext context, IPasswordHasher passwordHasher, ILogger? logger)
    {
        logger?.LogInformation("Seeding admin and manager accounts...");

        // Admin chính
        var (adminHash, adminSalt) = passwordHasher.HashPassword("Admin@123456");
        var adminAccount = Account.Create(
            "admin@zentry.com",
            adminHash,
            adminSalt,
            "Admin" // Changed from SuperAdmin to Admin
        );
        var adminUser = User.Create(adminAccount.Id, "System Administrator", "+84901234567");

        // Manager account
        var (managerHash, managerSalt) = passwordHasher.HashPassword("Manager@123456");
        var managerAccount = Account.Create(
            "manager@zentry.com",
            managerHash,
            managerSalt,
            "Manager"
        );
        var managerUser = User.Create(managerAccount.Id, "Nguyễn Văn Manager", "+84901234569");

        await context.Accounts.AddRangeAsync(adminAccount, managerAccount);
        await context.Users.AddRangeAsync(adminUser, managerUser);

        logger?.LogInformation("Added 2 core accounts (Admin, Manager)");
    }

    private static async Task SeedRegularUsers(UserDbContext context, IPasswordHasher passwordHasher, ILogger? logger)
    {
        logger?.LogInformation("Seeding Lecturer and Student users with Bogus...");

        try
        {
            Randomizer.Seed = new Random(100);

            var vietnameseNames = new[]
            {
                "Nguyễn Văn An", "Trần Thị Bình", "Lê Văn Cường", "Phạm Thị Dung", "Hoàng Văn Em",
                "Đặng Thị Fang", "Vũ Văn Giang", "Bùi Thị Hoa", "Đỗ Văn Inh", "Ngô Thị Khánh",
                "Đinh Văn Long", "Lý Thị Mai", "Tôn Văn Nam", "Phan Thị Oanh", "Hồ Văn Phúc",
                "Chu Thị Quỳnh", "La Văn Rồng", "Mai Thị Sương", "Tạ Văn Tâm", "Cao Thị Uyên",
                "Dương Văn Vinh", "Lưu Thị Wyn", "Khương Văn Xuân", "Ông Thị Yến", "Âu Văn Zũ"
            };

            var companyDomains = new[]
            {
                "gmail.com", "yahoo.com", "outlook.com", "edu.vn", "fpt.edu.vn"
            };

            var rolesForRegularUsers = new[] { "Lecturer", "Student" };

            var accountsAndUsers = new List<(Account Account, User User)>(); // Dùng tuple để giữ cả account và user

            var accountCount = 2;

            for (int i = 0; i < accountCount; i++)
            {
                var faker = new Faker(); // Tạo Faker riêng cho mỗi lần lặp để đảm bảo tính ngẫu nhiên tốt hơn nếu cần

                var name = faker.PickRandom(vietnameseNames);
                var emailName = RemoveDiacritics(name)
                    .ToLowerInvariant()
                    .Replace(" ", ".")
                    .Replace("đ", "d")
                    .Replace("ô", "o")
                    .Replace("ư", "u");

                // Để đảm bảo email duy nhất, hãy thêm một chỉ mục hoặc GUID
                // Ví dụ: sử dụng faker.UniqueIndex để tạo chỉ mục duy nhất
                var email = $"{emailName}.{faker.IndexFaker}@test.com";
                var role = faker.PickRandom(rolesForRegularUsers);

                var (hash, salt) = passwordHasher.HashPassword("User@123456");
                var account = Account.Create(email, hash, salt, role);

                var phoneNumber = faker.Random.Bool(0.8f) // 80% có số điện thoại
                    ? $"+849{faker.Random.Number(10000000, 99999999)}"
                    : null;
                var user = User.Create(account.Id, name, phoneNumber);

                // Cập nhật trạng thái cho một số account
                if (i < 10) // 10% đầu tiên là Inactive
                    account.UpdateStatus(AccountStatus.Inactive);
                else if (i >= 10 && i < 15) // 5% tiếp theo là Locked
                    account.UpdateStatus(AccountStatus.Locked);

                accountsAndUsers.Add((account, user));
            }

            // Tách danh sách accounts và users từ tuple
            var accounts = accountsAndUsers.Select(x => x.Account).ToList();
            var users = accountsAndUsers.Select(x => x.User).ToList();


            await context.Accounts.AddRangeAsync(accounts);
            await context.Users.AddRangeAsync(users);
            logger?.LogInformation($"Added {accounts.Count} regular users (Lecturer/Student) with Bogus data");
        }
        catch (Exception e)
        {
            Console.WriteLine(e); // Hoặc logger.LogError(e, "Error during regular user seeding");
            throw;
        }
    }

    private static async Task SeedTestAccounts(UserDbContext context, IPasswordHasher passwordHasher, ILogger? logger)
    {
        logger?.LogInformation("Seeding test accounts for development...");

        // Test accounts với mật khẩu đơn giản cho development
        var testAccounts = new[]
        {
            new { Email = "teststudent@test.com", Name = "Test Student", Role = "Student", Phone = "+84901111111" },
            new { Email = "testlecturer@test.com", Name = "Test Lecturer", Role = "Lecturer", Phone = "+84901111112" },
            new
            {
                Email = "inactiveuser@test.com", Name = "Inactive User Test", Role = "Student", Phone = "+84901111113"
            },
            new { Email = "lockeduser@test.com", Name = "Locked User Test", Role = "Lecturer", Phone = "+84901111114" },
            new
            {
                Email = "expiredreset@test.com", Name = "Expired Reset User", Role = "Student", Phone = "+84901111115"
            }
        };

        var accounts = new List<Account>();
        var users = new List<User>();

        foreach (var testData in testAccounts)
        {
            var (hash, salt) = passwordHasher.HashPassword("Test@123456");
            var account = Account.Create(testData.Email, hash, salt, testData.Role);
            var user = User.Create(account.Id, testData.Name, testData.Phone);

            // Thiết lập trạng thái đặc biệt
            if (testData.Email.Contains("inactiveuser"))
                account.UpdateStatus(AccountStatus.Inactive);
            else if (testData.Email.Contains("lockeduser"))
                account.UpdateStatus(AccountStatus.Locked);
            else if (testData.Email.Contains("expiredreset"))
            {
                // Tạo reset token đã hết hạn
                account.SetResetToken(Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(-1));
            }

            accounts.Add(account);
            users.Add(user);
        }

        await context.Accounts.AddRangeAsync(accounts);
        await context.Users.AddRangeAsync(users);

        logger?.LogInformation("Added 5 test accounts for development");
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    public static async Task ClearAllData(UserDbContext context, ILogger? logger = null)
    {
        logger?.LogInformation("Clearing all User Management data...");
        context.Users.RemoveRange(context.Users);
        context.Accounts.RemoveRange(context.Accounts);
        await context.SaveChangesAsync();
        logger?.LogInformation("All User Management data cleared.");
    }

    public static async Task ReseedAsync(UserDbContext context, IPasswordHasher passwordHasher, ILogger? logger = null)
    {
        await ClearAllData(context, logger);
        await SeedAsync(context, passwordHasher, logger);
    }
}
