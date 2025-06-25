using MediatR;

namespace Zentry.Modules.UserManagement.Features.ResetPassword;

public record ConfirmResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;
