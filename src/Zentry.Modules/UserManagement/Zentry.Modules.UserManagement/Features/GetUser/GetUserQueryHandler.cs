using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.GetUser;

public class GetUserQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        // 1. Tìm User
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user == null) return null; // Hoặc ném ngoại lệ NotFoundException, tùy thuộc vào chiến lược lỗi của bạn

        // 2. Tìm Account liên quan (sử dụng phương thức mới)
        var account = await userRepository.GetAccountByUserId(query.UserId);
        if (account == null)
            // Trường hợp này không nên xảy ra nếu dữ liệu hợp lệ (User luôn có Account)
            return null;

        // 3. Ánh xạ từ Entities sang Response DTO
        var response = new GetUserResponse
        {
            UserId = user.Id,
            AccountId = account.Id,
            Email = account.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = account.Role,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt
        };

        return response;
    }
}
