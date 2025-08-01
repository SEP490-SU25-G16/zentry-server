using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.UserManagement.Integration;

public class GetUsersByRoleQueryHandler(UserDbContext userDbContext)
    : IQueryHandler<GetUsersByRoleIntegrationQuery, GetUsersByRoleIntegrationResponse>
{
    public async Task<GetUsersByRoleIntegrationResponse> Handle(GetUsersByRoleIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        var accountIdsWithRole = await userDbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Role == request.Role && a.Status == AccountStatus.Active)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var userIds = await userDbContext.Users
            .AsNoTracking()
            .Where(u => accountIdsWithRole.Contains(u.AccountId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
        return new GetUsersByRoleIntegrationResponse(userIds);
    }
}