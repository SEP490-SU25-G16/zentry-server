namespace Zentry.Modules.UserManagement.Features.SignIn;

public record SignInResponse(string Token, UserInfo UserInfo);

public record UserInfo(Guid Id, string Email, string Role);
