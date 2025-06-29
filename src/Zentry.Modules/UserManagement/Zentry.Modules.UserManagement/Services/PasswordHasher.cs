using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Zentry.Modules.UserManagement.Services;

public class PasswordHasher : IPasswordHasher
{
    // Cấu hình các tham số cho Argon2id.
    // Đây là các giá trị khuyến nghị ban đầu, có thể điều chỉnh tùy theo yêu cầu bảo mật và tài nguyên hệ thống.
    private const int Iterations = 4; // Số lần lặp
    private const int MemorySize = 1024 * 1024; // 1GB RAM (tính bằng KB, 1024KB = 1MB)
    private const int Parallelism = 4; // Số luồng CPU
    private const int SaltSize = 16; // 16 bytes cho salt (recommended by OWASP)
    private const int HashSize = 32; // 32 bytes cho hash (tương đương 256 bits)

    public (string HashedPassword, string Salt) HashPassword(string password)
    {
        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt); // Sinh salt ngẫu nhiên và an toàn

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = Iterations,
            MemorySize = MemorySize,
            DegreeOfParallelism = Parallelism
        };

        byte[] hash = argon2.GetBytes(HashSize);

        // Chuyển đổi byte array sang Base64 string để lưu vào DB
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyHashedPassword(string storedHashedPassword, string storedSalt, string providedPassword)
    {
        try
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            byte[] storedHash = Convert.FromBase64String(storedHashedPassword);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(providedPassword))
            {
                Salt = salt,
                Iterations = Iterations,
                MemorySize = MemorySize,
                DegreeOfParallelism = Parallelism
            };

            byte[] computedHash = argon2.GetBytes(HashSize);

            // So sánh an toàn thời gian để ngăn chặn tấn công timing attacks
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có vấn đề trong quá trình xác thực (ví dụ: định dạng Base64 không hợp lệ)
            Console.WriteLine($"Error during Argon2 verification: {ex.Message}");
            return false;
        }
    }
}
