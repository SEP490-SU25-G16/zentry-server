using System.Globalization;
using System.Text;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Enums;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services;

// For IPasswordHasher

namespace Zentry.Modules.UserManagement.Persistence.Data;

public static class UserSeedData
{
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

            // --- TỐI ƯU 1: Hash mật khẩu mặc định chỉ một lần ---
            // Mật khẩu chung cho tất cả các tài khoản fake trong môi trường dev
            const string defaultPassword = "User@123456";
            var (defaultHash, defaultSalt) = passwordHasher.HashPassword(defaultPassword);
            logger?.LogInformation("Default password for fake accounts has been hashed once.");

            // 1. Seed Admin Accounts (Real data)
            await SeedAdminAccounts(context, passwordHasher, logger); // Admin và Manager vẫn dùng mật khẩu riêng

            // 2. Seed Regular Users (Lecturer and Student) với Bogus (Fake data)
            // Truyền mật khẩu hash đã tạo vào đây
            await SeedRegularUsers(context, defaultHash, defaultSalt, logger);

            // 3. Seed Test Accounts cho development (Nếu bạn vẫn cần)
            // Test accounts cũng dùng mật khẩu hash đã tạo
            await SeedTestAccounts(context, defaultHash, defaultSalt, logger);

            // TỐI ƯU 2: Chỉ gọi SaveChangesAsync một lần ở cuối SeedAsync
            // để batch tất cả các thao tác thêm vào database.
            // Điều này giảm thiểu round-trip đến database.
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
            "Admin"
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

        // Lưu ý: Không gọi SaveChangesAsync ở đây. Nó sẽ được gọi ở cuối SeedAsync
        logger?.LogInformation("Added 2 core accounts (Admin, Manager)");
    }

    // UPDATED: Thêm tham số defaultHash và defaultSalt
    private static async Task SeedRegularUsers(UserDbContext context, string defaultHash, string defaultSalt,
        ILogger? logger = null)
    {
        logger?.LogInformation("Seeding Lecturer and Student users with Bogus...");

        try
        {
            Randomizer.Seed = new Random(100); // Vẫn giữ seed để tái tạo

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

            var accountsToSeed = new List<Account>();
            var usersToSeed = new List<User>();

            var accountCount = 100; // Số lượng người dùng bạn muốn tạo

            // TỐI ƯU 3: Tạo một Faker instance duy nhất
            var faker = new Faker();

            for (var i = 0; i < accountCount; i++)
            {
                var name = faker.PickRandom(vietnameseNames);
                var emailName = RemoveDiacritics(name)
                    .ToLowerInvariant()
                    .Replace(" ", ".")
                    .Replace("đ", "d")
                    .Replace("ô", "o")
                    .Replace("ư", "u");

                // TỐI ƯU 4: Đảm bảo email duy nhất và hiệu quả hơn
                // Dùng GUID cho phần duy nhất, vì faker.IndexFaker có thể trùng nếu có nhiều instances của Faker được tạo.
                // Hoặc dùng faker.UniqueIndex() nếu bạn sử dụng một Faker<T> duy nhất như cách tôi đã gợi ý trước đó.
                var email =
                    $"{emailName}.{Guid.NewGuid().ToString("N").Substring(0, 8)}@{faker.PickRandom(companyDomains)}";

                var role = faker.PickRandom(rolesForRegularUsers);

                // --- TỐI ƯU 1 Áp dụng: Sử dụng mật khẩu hash đã tạo sẵn ---
                var account = Account.Create(email, defaultHash, defaultSalt, role);

                var phoneNumber = faker.Random.Bool(0.8f) // 80% có số điện thoại
                    ? $"+849{faker.Random.Number(10000000, 99999999)}"
                    : null;
                var user = User.Create(account.Id, name, phoneNumber);

                // Cập nhật trạng thái cho một số account
                if (i < 10) // 10% đầu tiên là Inactive
                    account.UpdateStatus(AccountStatus.Inactive);
                else if (i >= 10 && i < 15) // 5% tiếp theo là Locked
                    account.UpdateStatus(AccountStatus.Locked);

                accountsToSeed.Add(account);
                usersToSeed.Add(user);
            }

            await context.Accounts.AddRangeAsync(accountsToSeed);
            await context.Users.AddRangeAsync(usersToSeed);

            // Lưu ý: Không gọi SaveChangesAsync ở đây. Nó sẽ được gọi ở cuối SeedAsync
            logger?.LogInformation($"Added {accountsToSeed.Count} regular users (Lecturer/Student) with Bogus data");
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error during regular user seeding");
            throw;
        }
    }

    // UPDATED: Thêm tham số defaultHash và defaultSalt
    private static async Task SeedTestAccounts(UserDbContext context, string defaultHash, string defaultSalt,
        ILogger? logger = null)
    {
        logger?.LogInformation("Seeding test accounts for development...");

        var testAccountsData = new[]
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

        foreach (var testData in testAccountsData)
        {
            // --- TỐI ƯU 1 Áp dụng: Sử dụng mật khẩu hash đã tạo sẵn ---
            var account = Account.Create(testData.Email, defaultHash, defaultSalt, testData.Role);
            var user = User.Create(account.Id, testData.Name, testData.Phone);

            // Thiết lập trạng thái đặc biệt
            if (testData.Email.Contains("inactiveuser"))
                account.UpdateStatus(AccountStatus.Inactive);
            else if (testData.Email.Contains("lockeduser"))
                account.UpdateStatus(AccountStatus.Locked);
            else if (testData.Email.Contains("expiredreset"))
                // Tạo reset token đã hết hạn
                account.SetResetToken(Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(-1));

            accounts.Add(account);
            users.Add(user);
        }

        await context.Accounts.AddRangeAsync(accounts);
        await context.Users.AddRangeAsync(users);

        // Lưu ý: Không gọi SaveChangesAsync ở đây. Nó sẽ được gọi ở cuối SeedAsync
        logger?.LogInformation("Added 5 test accounts for development");
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark) stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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