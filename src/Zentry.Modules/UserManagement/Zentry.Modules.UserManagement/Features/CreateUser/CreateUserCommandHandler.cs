using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;

namespace Zentry.Modules.UserManagement.Features.CreateUser;

public class CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailExists = await userRepository.ExistsByEmail(command.Email);
        if (emailExists)
            throw new InvalidOperationException($"Email '{command.Email}' đã tồn tại.");
        var (hashedPassword, salt) = passwordHasher.HashPassword(command.Password);

        var account = Account.Create(command.Email, hashedPassword, salt, Role.FromName(command.Role));
        var user = User.Create(account.Id, command.FullName, command.PhoneNumber);

        await userRepository.AddAsync(account, user, cancellationToken);

        return new CreateUserResponse
        {
            UserId = user.Id,
            AccountId = account.Id,
            Email = account.Email,
            FullName = user.FullName,
            Role = account.Role.ToString(),
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt
        };
    }
}
