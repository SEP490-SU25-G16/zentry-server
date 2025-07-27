using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public class SignInHandler(UserDbContext dbContext, IJwtService jwtService, IPasswordHasher passwordHasher)
    : ICommandHandler<SignInCommand, SignInResponse>
{
    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        if (account is null) throw new UnauthorizedAccessException("Invalid credentials.");

        if (!Equals(account.Status, AccountStatus.Active))
            // Trả về lỗi phù hợp với trạng thái tài khoản
            throw account.Status.Id switch
            {
                2 => new UnauthorizedAccessException("Account is inactive."),
                3 => new UnauthorizedAccessException("Account is locked."),
                _ => throw new ArgumentOutOfRangeException()
            };


        if (string.IsNullOrEmpty(account.PasswordHash) || string.IsNullOrEmpty(account.PasswordSalt) ||
            !passwordHasher.VerifyHashedPassword(account.PasswordHash, account.PasswordSalt, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var user = await dbContext.Users.Where(u => u.AccountId == account.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) throw new BadHttpRequestException("There are something wrong at server side.");

        var token = jwtService.GenerateToken(user.Id, account.Email, user.FullName, account.Role.ToString());
        return new SignInResponse(token, new UserInfo(user.Id, account.Email, user.FullName, account.Role.ToString()));
    }
}
