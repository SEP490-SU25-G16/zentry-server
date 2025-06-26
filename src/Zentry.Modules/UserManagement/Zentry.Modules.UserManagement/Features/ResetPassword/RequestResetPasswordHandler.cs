using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Persistence;
using Zentry.Modules.UserManagement.Services;

namespace Zentry.Modules.UserManagement.Features.ResetPassword;

public class RequestResetPasswordHandler(UserDbContext dbContext, IEmailService emailService)
    : IRequestHandler<RequestResetPasswordCommand>
{
    public async Task Handle(RequestResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        if (account == null)
        {
            // For security reasons, always send a success response even if the email doesn't exist
            // to prevent email enumeration.
            return;
        }

        var token = Guid.NewGuid().ToString("N"); // Simple token generation
        var expiryTime = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

        // Sử dụng phương thức SetResetToken của entity Account
        account.SetResetToken(token, expiryTime);

        await dbContext.SaveChangesAsync(cancellationToken);

        var emailBody = $"Your password reset token is: {token}. It is valid for 1 hour. " +
                        $"Please use this token to reset your password on our website."; // Added more context
        await emailService.SendEmailAsync(request.Email, "Password Reset Request for Zentry Account", emailBody);
    }
}
