using MediatR;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Features.SignIn;

public record SignInCommand(string Email, string Password) : ICommand<SignInResponse>;
