using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.UserManagement.Services;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
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
}
