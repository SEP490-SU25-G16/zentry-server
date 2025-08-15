using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public class SignInHandler(
    UserDbContext dbContext, 
    IJwtService jwtService, 
    IPasswordHasher passwordHasher,
    IMediator mediator,
    ISessionService sessionService) : ICommandHandler<SignInCommand, SignInResponse>
{
    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        // Tìm account
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        if (account is null)
            throw new AccountNotFoundException("Account not found.");

        if (!Equals(account.Status, AccountStatus.Active))
            throw account.Status.Id switch
            {
                2 => new AccountInactiveException("Account is inactive."),
                3 => new AccountLockedException("Account is locked."),
                _ => new AccountDisabledException("Account is disabled.")
            };

        // Kiểm tra password
        if (string.IsNullOrEmpty(account.PasswordHash) ||
            string.IsNullOrEmpty(account.PasswordSalt) ||
            !passwordHasher.VerifyHashedPassword(account.PasswordHash, account.PasswordSalt, request.Password))
            throw new InvalidCredentialsException("Invalid email or password.");

        // Tìm user
        var user = await dbContext.Users
            .Where(u => u.AccountId == account.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User data not found for this account.");

        // ✅ THÊM: Device validation
        if (string.IsNullOrEmpty(request.DeviceToken))
        {
            throw new BusinessRuleException("DEVICE_TOKEN_REQUIRED", 
                "Device token là bắt buộc để đăng nhập.");
        }

        var deviceQuery = new GetDeviceByTokenIntegrationQuery(request.DeviceToken);
        var deviceResponse = await mediator.Send(deviceQuery, cancellationToken);

        if (deviceResponse.Device == null)
        {
            throw new BusinessRuleException("DEVICE_NOT_REGISTERED", 
                "Thiết bị chưa được đăng ký. Vui lòng đăng ký thiết bị trước khi đăng nhập.");
        }

        if (deviceResponse.Device.Status != "Active")
        {
            throw new BusinessRuleException("DEVICE_NOT_ACTIVE", 
                "Thiết bị không ở trạng thái hoạt động. Vui lòng liên hệ admin.");
        }

        if (deviceResponse.Device.UserId != user.Id)
        {
            throw new BusinessRuleException("DEVICE_NOT_OWNED", 
                "Thiết bị không thuộc về tài khoản này.");
        }

        // ✅ Kiểm tra user đã có active session chưa
        if (await sessionService.HasActiveSessionAsync(user.Id))
        {
            // Force logout session cũ
            await sessionService.RevokeAllUserSessionsAsync(user.Id);
        }

        // ✅ Tạo session thay vì JWT
        var sessionKey = await sessionService.CreateSessionAsync(
            user.Id, 
            deviceResponse.Device.Id, 
            TimeSpan.FromMinutes(30) // Session 30 phút
        );

        return new SignInResponse
        {
            SessionKey = sessionKey,
            UserInfo = new UserInfo
            {
                Id = user.Id,
                Email = account.Email,
                FullName = user.FullName,
                Role = account.Role.ToString()
            },
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }
}