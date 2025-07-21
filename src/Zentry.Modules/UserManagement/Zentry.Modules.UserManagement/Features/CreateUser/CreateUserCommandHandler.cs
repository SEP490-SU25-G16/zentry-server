using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Services;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Features.CreateUser;

public class CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xem email đã tồn tại chưa
        var emailExists = await userRepository.ExistsByEmail(command.Email);
        if (emailExists)
            // Tùy chọn: ném exception hoặc trả về lỗi cụ thể
            throw new InvalidOperationException($"Email '{command.Email}' đã tồn tại.");
        // Hoặc nếu bạn muốn mô hình Result<T> để xử lý lỗi một cách graceful hơn
        // return new CreateUserResponse { Success = false, ErrorMessage = "Email already exists" };
        // 2. Băm mật khẩu
        var (hashedPassword, salt) = passwordHasher.HashPassword(command.Password);

        // 3. Tạo đối tượng Account và User
        // Sử dụng phương thức Create tĩnh từ entity
        var account = Account.Create(command.Email, hashedPassword, salt, command.Role);
        var user = User.Create(account.Id, command.FullName, command.PhoneNumber);

        // 4. Lưu vào cơ sở dữ liệu
        await userRepository.AddAsync(account, user, cancellationToken);

        // 5. Trả về Response
        return new CreateUserResponse
        {
            UserId = user.Id,
            AccountId = account.Id,
            Email = account.Email,
            FullName = user.FullName,
            Role = account.Role,
            Status = account.Status,
            CreatedAt = account.CreatedAt
        };
    }
}