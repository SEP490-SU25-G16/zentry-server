using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.DeleteUser;

public class DeleteUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<DeleteUserCommand, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await userRepository.SoftDeleteUserAsync(command.UserId, cancellationToken);
            return new DeleteUserResponse { Success = true, Message = "User soft deleted successfully." };
        }
        catch (InvalidOperationException ex)
        {
            // Xử lý các lỗi cụ thể từ repository (ví dụ: User not found)
            return new DeleteUserResponse { Success = false, Message = ex.Message };
        }
        catch (Exception)
        {
            // Xử lý các lỗi chung khác
            return new DeleteUserResponse { Success = false, Message = "An error occurred during soft delete." };
        }
    }
}