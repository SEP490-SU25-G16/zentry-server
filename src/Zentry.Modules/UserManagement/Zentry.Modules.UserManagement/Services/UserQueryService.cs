using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.UserManagement.Services;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService, IUserDeviceService
{
    public Task<bool> CheckUserExistsAsync(Guid userId)
    {
        // *** IMPORTANT: Replace with actual logic to check user existence in the User module. ***
        // Example: return await _identityDbContext.Users.AnyAsync(u => u.Id == userId);
        // For now, a simple mock:
        return Task.FromResult(userId != Guid.Empty); // Assuming Guid.Empty is an invalid user
    }

    public async Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null) return null;

        return new LecturerLookupDto
        {
            Id = user.Id,
            FullName = user.FullName
        };
    }

    public async Task<UserLookupDto?> GeUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        // *** Thay thế bằng logic thực tế để lấy thông tin người dùng từ Identity Module. ***
        // Ví dụ: Query DBContext của Identity, hoặc gọi một Http client đến Identity API.
        // var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        // if (user == null) return null;
        //
        // return new UserLookupDto
        // {
        //     Id = user.Id,
        //     Name = user.FullName
        // };
        // Dữ liệu mock để thử nghiệm:
        if (userId == new Guid("00000000-0000-0000-0000-000000000001")) // Giả lập Admin User
            return await Task.FromResult<UserLookupDto?>(new UserLookupDto { Id = userId, Name = "Admin User" });
        if (userId == new Guid("00000000-0000-0000-0000-000000000002")) // Giả lập Student User
            return await Task.FromResult<UserLookupDto?>(new UserLookupDto
                { Id = userId, Name = "Student A", StudentCode = "ST001" });
        if (userId == new Guid("00000000-0000-0000-0000-000000000003")) // Giả lập non-student/non-admin user
            return await Task.FromResult<UserLookupDto?>(new UserLookupDto { Id = userId, Name = "Teacher B" });

        return await Task.FromResult<UserLookupDto?>(null); // Không tìm thấy user
    }
}