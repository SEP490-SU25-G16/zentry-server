using MediatR;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Configuration;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.UserManagement.Integration;

public class GetUsersByIdsIntegrationQueryHandler(IUserRepository userRepository, IMediator mediator)
    : IQueryHandler<GetUsersByIdsIntegrationQuery, GetUsersByIdsIntegrationResponse>
{
    public async Task<GetUsersByIdsIntegrationResponse> Handle(
        GetUsersByIdsIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UserIds.Count == 0) return new GetUsersByIdsIntegrationResponse(new List<BasicUserInfoDto>());

        var users = await userRepository.GetUsersByIdsAsync(request.UserIds, cancellationToken);

        var userAttributesQuery = new GetUserAttributesForUsersIntegrationQuery(users.Select(u => u.Id).ToList());
        var userAttributesResponse = await mediator.Send(userAttributesQuery, cancellationToken);

        var dtos = users.Select(u =>
        {
            var attributes = userAttributesResponse.UserAttributes.GetValueOrDefault(u.Id, new Dictionary<string, string>());
            return new BasicUserInfoDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Account?.Email,
                Attributes = attributes
            };
        }).ToList();

        return new GetUsersByIdsIntegrationResponse(dtos);
    }
}
