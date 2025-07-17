using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public record SignInCommand(string Email, string Password) : ICommand<SignInResponse>;

public record SignInResponse(string Token, UserInfo UserInfo);

public record UserInfo(Guid Id, string Email, string Role);
