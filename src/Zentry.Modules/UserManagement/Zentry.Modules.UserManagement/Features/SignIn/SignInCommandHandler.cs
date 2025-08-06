using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public class SignInHandler(UserDbContext dbContext, IJwtService jwtService, IPasswordHasher passwordHasher)
    : ICommandHandler<SignInCommand, SignInResponse>
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

        var token = jwtService.GenerateToken(user.Id, account.Email, user.FullName, account.Role.ToString());
        return new SignInResponse(token, new UserInfo(user.Id, account.Email, user.FullName, account.Role.ToString()));
    }
}