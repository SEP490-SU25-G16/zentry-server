using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Persistence;
using Zentry.Modules.UserManagement.Services;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public class SignInHandler(UserDbContext dbContext, IJwtService jwtService, IArgon2PasswordHasher passwordHasher)
    : IRequestHandler<SignInCommand, SignInResponse>
{
    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        // For security reasons, don't distinguish between invalid email and invalid password
        if (account == null || string.IsNullOrEmpty(account.PasswordHash) || string.IsNullOrEmpty(account.PasswordSalt) ||
            !passwordHasher.VerifyHashedPassword(account.PasswordHash, account.PasswordSalt, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = jwtService.GenerateToken(account.Id, account.Email, account.Role);

        return new SignInResponse(token, new UserInfo(account.Id, account.Email, account.Role));
    }
}
