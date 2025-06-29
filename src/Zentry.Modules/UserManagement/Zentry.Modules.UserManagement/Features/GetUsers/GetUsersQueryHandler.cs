using Zentry.Modules.UserManagement.Interfaces;
using Zentry.Modules.UserManagement.Services;
using Zentry.SharedKernel.Abstractions.Application; // Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.GetUsers;

public class GetUsersQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUsersQuery, GetUsersResponse>
{
    public async Task<GetUsersResponse> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await userRepository.GetUsersAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm,
            query.Role,
            query.Status
        );

        return new GetUsersResponse
        {
            Users = users,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
