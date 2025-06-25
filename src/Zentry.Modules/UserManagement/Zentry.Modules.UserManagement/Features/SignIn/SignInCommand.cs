using MediatR;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public record SignInCommand(string Email, string Password) : IRequest<SignInResponse>;
