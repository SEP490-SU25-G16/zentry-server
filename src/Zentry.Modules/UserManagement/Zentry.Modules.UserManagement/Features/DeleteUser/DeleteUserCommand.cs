using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.DeleteUser;

// Command để thực hiện soft delete người dùng
public class DeleteUserCommand(Guid userId) : ICommand<DeleteUserResponse>
{
    public Guid UserId { get; init; } = userId;
}
