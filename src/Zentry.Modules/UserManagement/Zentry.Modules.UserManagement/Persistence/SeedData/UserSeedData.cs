using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services;
using Zentry.SharedKernel.Enums;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.UserManagement.Persistence.SeedData;

public static class UserSeedData
{
    private const int StudentCount = 100;
    private const int LecturerCount = 10;

    public static async Task SeedAsync(UserDbContext context, IPasswordHasher passwordHasher, ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Starting User Management seed data...");

            if (await context.Accounts.AnyAsync())
            {
                logger?.LogInformation("User Management data already exists. Skipping seed.");
                return;
            }

            const string defaultPassword = "User@123456";
            var (defaultHash, defaultSalt) = passwordHasher.HashPassword(defaultPassword);
            logger?.LogInformation("Default password for fake accounts has been hashed once.");

            await SeedAdminAccounts(context, defaultHash, defaultSalt, logger);
            await SeedLecturers(context, defaultHash, defaultSalt, logger);
            await SeedStudents(context, defaultHash, defaultSalt, logger);

            await context.SaveChangesAsync();
            logger?.LogInformation("User Management seed data completed successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding User Management data");
            throw;
        }
    }

    private static async Task SeedAdminAccounts(UserDbContext context, string defaultHash, string defaultSalt,
        ILogger? logger)
    {
        logger?.LogInformation("Seeding admin and manager accounts...");

        var adminAccount = Account.Create(
            "admin@zentry.com",
            defaultHash,
            defaultSalt,
            Role.Admin
        );
        var adminUser = User.Create(adminAccount.Id, "System Administrator", "+84901234567");

        var managerAccount = Account.Create(
            "manager@zentry.com",
            defaultHash,
            defaultSalt,
            Role.Manager
        );
        var managerUser = User.Create(managerAccount.Id, "Nguyễn Văn Manager", "+84901234569");

        await context.Accounts.AddRangeAsync(adminAccount, managerAccount);
        await context.Users.AddRangeAsync(adminUser, managerUser);
        logger?.LogInformation("Added 2 core accounts (Admin, Manager)");
    }

    private static async Task SeedLecturers(UserDbContext context, string defaultHash, string defaultSalt,
        ILogger? logger = null)
    {
        logger?.LogInformation("Seeding Lecturer users with Bogus...");

        try
        {
            Randomizer.Seed = new Random(300);

            var vietnameseNames = new[]
            {
                "Phạm Văn Nam", "Nguyễn Thị Thảo", "Lê Đình Anh", "Trần Mai Phương", "Hoàng Kim Sơn",
                "Đỗ Thị Hà", "Vũ Minh Đức", "Bùi Thu Trang", "Đinh Hữu Lộc", "Ngô Thanh Nga",
                "Đặng Xuân Trường", "Lý Hoài An", "Tôn Gia Bảo", "Phan Cẩm Tú", "Hồ Quang Vinh",
                "Chu Văn Long", "La Thị Thanh", "Mai Văn Cường", "Tạ Thị Hạnh", "Cao Minh Quân",
                "Dương Thị Yến", "Lưu Thanh Tùng", "Khương Thị Diệu", "Ông Văn Trung", "Âu Thị Kim Anh",
                "Trịnh Văn Khoa", "Đào Thu Hiền", "Lâm Bá Duy", "Võ Thị Quỳnh Chi", "Huỳnh Thanh Phúc"
            };

            var accountsToSeed = new List<Account>();
            var usersToSeed = new List<User>();

            var faker = new Faker();

            for (var i = 0; i < LecturerCount; i++)
            {
                var name = faker.PickRandom(vietnameseNames);
                // --- THAY ĐỔI Ở ĐÂY ---
                var emailName = SanitizeEmailName(name); // Sử dụng hàm mới để đảm bảo không dấu và hợp lệ
                // --- KẾT THÚC THAY ĐỔI ---

                var email =
                    $"{emailName}.lecturer{i}@zentry.edu";

                var account = Account.Create(email, defaultHash, defaultSalt, Role.Lecturer);

                var phoneNumber = $"+849{faker.Random.Number(10000000, 99999999)}";
                var user = User.Create(account.Id, name, phoneNumber);

                if (i < 5) account.UpdateStatus(AccountStatus.Inactive);

                accountsToSeed.Add(account);
                usersToSeed.Add(user);
            }

            await context.Accounts.AddRangeAsync(accountsToSeed);
            await context.Users.AddRangeAsync(usersToSeed);
            logger?.LogInformation($"Added {accountsToSeed.Count} Lecturer users.");
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error during Lecturer seeding");
            throw;
        }
    }

    private static async Task SeedStudents(UserDbContext context, string defaultHash, string defaultSalt,
        ILogger? logger = null)
    {
        logger?.LogInformation("Seeding Student users with Bogus...");

        try
        {
            Randomizer.Seed = new Random(301);

            var vietnameseNames = new[]
            {
                "Nguyễn Văn An", "Trần Thị Bình", "Lê Văn Cường", "Phạm Thị Dung", "Hoàng Văn Em",
                "Đặng Thị Fang", "Vũ Văn Giang", "Bùi Thị Hoa", "Đỗ Văn Inh", "Ngô Thị Khánh",
                "Đinh Văn Long", "Lý Thị Mai", "Tôn Văn Nam", "Phan Thị Oanh", "Hồ Văn Phúc",
                "Chu Thị Quỳnh", "La Văn Rồng", "Mai Thị Sương", "Tạ Văn Tâm", "Cao Thị Uyên",
                "Dương Văn Vinh", "Lưu Thị Wyn", "Khương Văn Xuân", "Ông Thị Yến", "Âu Văn Zũ",
                "Lê Thanh Tùng", "Phạm Thu Hương", "Trần Văn Luận", "Hoàng Thảo My", "Nguyễn Minh Khôi",
                "Đào Văn Minh", "Vũ Thị Ngân", "Bùi Tuấn Kiệt", "Dương Quốc Cường", "Phạm Hải Yến",
                "Nguyễn Đăng Khoa", "Trần Minh Đức", "Lê Thị Ngọc", "Phạm Thanh Tùng", "Hoàng Văn Khang",
                "Đặng Anh Duy", "Vũ Thị Thùy Linh", "Bùi Thị Phương Anh", "Đinh Mạnh Cường", "Ngô Đình Trọng",
                "Đào Duy Anh", "Lý Thị Mỹ Linh", "Tôn Nữ Diễm Quỳnh", "Phan Thị Ngọc Anh", "Hồ Văn Phát",
                "Chu Đình Phúc", "La Thị Thanh Thảo", "Mai Văn Hùng", "Tạ Thị Kim Ngân", "Cao Văn Khoa"
            };


            var companyDomains = new[]
            {
                "fpt.edu.vn"
            };

            var accountsToSeed = new List<Account>();
            var usersToSeed = new List<User>();

            var faker = new Faker();

            for (var i = 0; i < StudentCount; i++)
            {
                var name = faker.PickRandom(vietnameseNames);
                // --- THAY ĐỔI Ở ĐÂY ---
                var emailName = SanitizeEmailName(name); // Sử dụng hàm mới để đảm bảo không dấu và hợp lệ
                // --- KẾT THÚC THAY ĐỔI ---

                var email =
                    $"{emailName}.student{i}@{faker.PickRandom(companyDomains)}";

                var account = Account.Create(email, defaultHash, defaultSalt, Role.Student);

                var phoneNumber = faker.Random.Bool(0.6f)
                    ? $"+84{faker.Random.Number(300000000, 999999999)}"
                    : null;
                var user = User.Create(account.Id, name, phoneNumber);

                if (i % 10 == 0) account.UpdateStatus(AccountStatus.Locked);

                accountsToSeed.Add(account);
                usersToSeed.Add(user);
            }

            await context.Accounts.AddRangeAsync(accountsToSeed);
            await context.Users.AddRangeAsync(usersToSeed);
            logger?.LogInformation($"Added {accountsToSeed.Count} Student users.");
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error during Student seeding");
            throw;
        }
    }

    // Phương thức RemoveDiacritics đã có, nhưng chúng ta sẽ tạo một hàm mới để xử lý toàn bộ quá trình
    // chuyển đổi tên thành phần email name hợp lệ.
    private static string SanitizeEmailName(string text)
    {
        // 1. Loại bỏ dấu
        var noDiacritics = RemoveDiacritics(text);

        // 2. Chuyển thành chữ thường
        var lowerCase = noDiacritics.ToLowerInvariant();

        // 3. Thay thế khoảng trắng bằng dấu chấm
        var withDots = lowerCase.Replace(" ", ".");

        // 4. Loại bỏ các ký tự không hợp lệ cho email (ngoại trừ dấu chấm đã thêm)
        // Chỉ giữ lại chữ cái, số và dấu chấm
        var sanitized = Regex.Replace(withDots, "[^a-z0-9.]", "");

        // 5. Loại bỏ các dấu chấm liên tiếp và dấu chấm ở đầu/cuối
        sanitized = Regex.Replace(sanitized, "[.]+", "."); // Thay thế nhiều dấu chấm bằng một
        sanitized = sanitized.Trim('.'); // Loại bỏ dấu chấm ở đầu hoặc cuối

        return sanitized;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in from c in normalizedString
                 let unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c)
                 where unicodeCategory != UnicodeCategory.NonSpacingMark
                 select c)
            stringBuilder.Append(c);

        var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        result = result.Replace("đ", "d").Replace("Đ", "D");
        return result;
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
