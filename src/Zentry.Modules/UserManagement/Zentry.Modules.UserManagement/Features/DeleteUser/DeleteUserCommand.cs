using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.DeleteUser;

// Command để thực hiện soft delete người dùng
public class DeleteUserCommand : ICommand<DeleteUserResponse>
{
    public Guid UserId { get; init; }

    public DeleteUserCommand(Guid userId)
    {
        UserId = userId;
    }
}
