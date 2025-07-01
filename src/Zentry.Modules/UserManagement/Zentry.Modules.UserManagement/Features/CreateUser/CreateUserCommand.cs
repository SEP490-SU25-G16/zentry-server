using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Features.CreateUser;

public class CreateUserCommand : ICommand<CreateUserResponse>
{
    public CreateUserCommand(CreateUserRequest request)
    {
        Email = request.Email;
        Password = request.Password;
        FullName = request.FullName;
        PhoneNumber = request.PhoneNumber;
        Role = request.Role;
    }

    public string Email { get; init; }
    public string Password { get; init; }
    public string FullName { get; init; }
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = "User";
}