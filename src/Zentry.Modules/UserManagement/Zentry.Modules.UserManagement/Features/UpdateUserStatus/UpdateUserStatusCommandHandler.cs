using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Features.UpdateUserStatus;

public class UpdateUserStatusCommandHandler(IUserRepository userRepository)
    : ICommandHandler<UpdateUserStatusCommand, UpdateUserStatusResponse>
{
    public async Task<UpdateUserStatusResponse> Handle(UpdateUserStatusCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new ResourceNotFoundException("USER", command.UserId);

        var account = await userRepository.GetAccountById(user.AccountId);
        if (account is null)
            throw new ResourceNotFoundException("ACCOUNT", user.AccountId);

        AccountStatus newStatus;
        try
        {
            newStatus = AccountStatus.FromName(command.Request.Status);
        }
        catch (InvalidOperationException)
        {
            throw new BusinessRuleException("INVALID_STATUS", $"Invalid status value '{command.Request.Status}'.");
        }

        account.UpdateStatus(newStatus);

        await userRepository.UpdateAccountAsync(account, cancellationToken);

        return new UpdateUserStatusResponse { Success = true, Message = "User status updated successfully." };
    }
}