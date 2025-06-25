using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Persistence;
using Zentry.Modules.UserManagement.Services;

namespace Zentry.Modules.UserManagement.Features.ResetPassword;

public class ConfirmResetPasswordHandler(UserDbContext dbContext, IArgon2PasswordHasher passwordHasher)
    : IRequestHandler<ConfirmResetPasswordCommand>
{
    public async Task Handle(ConfirmResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == request.Email && a.ResetToken == request.Token, cancellationToken);

        // Check if account exists, token matches, and token is not expired
        if (account == null || account.ResetTokenExpiryTime == null || account.ResetTokenExpiryTime <= DateTime.UtcNow)
        {
            // IMPORTANT: If a token was found but expired/invalid, clear it to prevent further attempts.
            if (account != null)
            {
                account.ResetToken = null;
                account.ResetTokenExpiryTime = null;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            throw new InvalidOperationException("Invalid or expired token. Please request a new password reset.");
        }

        // Hash new password using Argon2id
        (account.PasswordHash, account.PasswordSalt) = passwordHasher.HashPassword(request.NewPassword);

        // Clear the token and expiry time after successful reset
        account.ResetToken = null;
        account.ResetTokenExpiryTime = null;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
