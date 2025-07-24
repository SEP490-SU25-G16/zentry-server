using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.UserManagement.Integration;

public class GetUsersByIdsIntegrationQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUsersByIdsIntegrationQuery, GetUsersByIdsIntegrationResponse>
{
    public async Task<GetUsersByIdsIntegrationResponse> Handle(
        GetUsersByIdsIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UserIds == null || !request.UserIds.Any())
        {
            return new GetUsersByIdsIntegrationResponse(new List<BasicUserInfoDto>());
        }

        // Giả sử IUserRepository có GetUsersByIdsAsync trả về List<User>
        var users = await userRepository.GetUsersByIdsAsync(request.UserIds, cancellationToken);

        var dtos = users.Select(u => new BasicUserInfoDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Account?.Email,
        }).ToList();

        return new GetUsersByIdsIntegrationResponse(dtos);
    }
}
