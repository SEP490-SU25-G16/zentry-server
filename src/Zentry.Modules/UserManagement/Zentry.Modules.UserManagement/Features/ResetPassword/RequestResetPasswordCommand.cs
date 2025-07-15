using MediatR;

namespace Zentry.Modules.UserManagement.Features.ResetPassword;

public record RequestResetPasswordCommand(string Email) : IRequest;