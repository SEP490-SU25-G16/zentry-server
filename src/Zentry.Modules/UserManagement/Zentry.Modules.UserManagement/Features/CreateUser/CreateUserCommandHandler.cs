// ... (Các using statements)

using MediatR;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Features.CreateUser;

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IMediator mediator)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailExists = await userRepository.IsExistsByEmail(null, command.Email);
        if (emailExists)
            throw new ResourceNotFoundException($"Email '{command.Email}' đã tồn tại.");

        var (hashedPassword, salt) = passwordHasher.HashPassword(command.Password);

        var account = Account.Create(command.Email, hashedPassword, salt, Role.FromName(command.Role));
        var user = User.Create(account.Id, command.FullName, command.PhoneNumber);

        await userRepository.AddAsync(account, user, cancellationToken);

        var userAttributes = new Dictionary<string, string>();
        if (Equals(account.Role, Role.Student))
        {
            var studentCode = await GenerateUniqueStudentCodeAsync(cancellationToken);
            userAttributes["StudentCode"] = studentCode;
        }
        else if (Equals(account.Role, Role.Lecturer))
        {
            var lecturerCode = await GenerateUniqueLecturerCodeAsync(cancellationToken);
            userAttributes["LecturerCode"] = lecturerCode;
        }

        var createAttributesCommand = new CreateUserAttributesIntegrationCommand(
            UserId: user.Id,
            UserAttributes: userAttributes
        );
        var integrationResponse = await mediator.Send(createAttributesCommand, cancellationToken);

        if (!integrationResponse.Success)
        {
            // Ngoại lệ mới: IntegrationException
            throw new IntegrationException($"Failed to create user attributes: {integrationResponse.Message}");
        }

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

    private Task<string> GenerateUniqueStudentCodeAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult($"S{new Random().Next(10000, 99999)}");
    }

    private Task<string> GenerateUniqueLecturerCodeAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult($"L{new Random().Next(1000, 9999)}");
    }
}
