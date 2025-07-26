using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.UserManagement.Features.UpdateUser;

// Chỉ cần kế thừa ICommandHandler của bạn
public class UpdateUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse> // TRƯỚC ĐÂY: IQueryHandler<UpdateUserCommand, UpdateUserResponse>
{
    // Phương thức Handle vẫn giữ nguyên tên và chữ ký như của MediatR.IQueryHandler
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null) return new UpdateUserResponse { Success = false, Message = "User not found." };

        var account = await userRepository.GetAccountById(user.AccountId);
        if (account == null)
            return new UpdateUserResponse { Success = false, Message = "Associated account not found." };

        user.UpdateUser(command.FullName, command.PhoneNumber);

        if (!string.IsNullOrWhiteSpace(command.Role)) account.UpdateAccount(role: Role.FromName(command.Role));

        await userRepository.UpdateAsync(user, cancellationToken);
        await userRepository.UpdateAccountAsync(account, cancellationToken);

        return new UpdateUserResponse { Success = true, Message = "User updated successfully." };
    }
}
