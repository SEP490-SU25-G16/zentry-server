using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Persistence;
using Zentry.Modules.UserManagement.Persistence.Enums;
using Zentry.Modules.UserManagement.Services;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public class SignInHandler(UserDbContext dbContext, IJwtService jwtService, IArgon2PasswordHasher passwordHasher)
    : IRequestHandler<SignInCommand, SignInResponse>
{
    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        // --- Bổ sung kiểm tra trạng thái tài khoản ---
        if (account == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials."); // Không tìm thấy email
        }

        if (account.Status != AccountStatus.Active)
        {
            // Trả về lỗi phù hợp với trạng thái tài khoản
            throw account.Status.Id switch
            {
                2 => new UnauthorizedAccessException("Account is inactive."),
                3 => new UnauthorizedAccessException("Account is locked."),
                _ => new UnauthorizedAccessException("Account is not active.")
            };
        }
        // --- Kết thúc bổ sung kiểm tra trạng thái tài khoản ---

        // For security reasons, don't distinguish between invalid email and invalid password
        if (string.IsNullOrEmpty(account.PasswordHash) || string.IsNullOrEmpty(account.PasswordSalt) ||
            !passwordHasher.VerifyHashedPassword(account.PasswordHash, account.PasswordSalt, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials."); // Sai mật khẩu
        }

        // Tạo JWT. Đảm bảo IJwtService.GenerateToken có thể nhận các tham số này
        var token = jwtService.GenerateToken(account.Id, account.Email, account.Role);

        // Ghi log hoạt động đăng nhập thành công (Bạn cần triển khai ILogger hoặc EventDispatcher ở đây)
        // Ví dụ: _logger.LogInformation("User {UserId} logged in successfully via Password.", account.Id);

        return new SignInResponse(token, new UserInfo(account.Id, account.Email, account.Role));
    }
}
