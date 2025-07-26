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
        if (request.UserIds.Count == 0) return new GetUsersByIdsIntegrationResponse(new List<BasicUserInfoDto>());

        // Giả sử GetUsersByIdsAsync có thể lấy được thông tin Account hoặc thông tin User trực tiếp có Phone Number
        var users = await userRepository.GetUsersByIdsAsync(request.UserIds, cancellationToken);

        var dtos = users.Select(u => new BasicUserInfoDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Account?.Email // Giả sử Email trên Account
        }).ToList();

        return new GetUsersByIdsIntegrationResponse(dtos);
    }
}