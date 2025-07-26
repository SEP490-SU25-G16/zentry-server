using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Integration;

public class GetUserByIdAndRoleQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserByIdAndRoleIntegrationQuery, GetUserByIdAndRoleIntegrationResponse>
{
    public async Task<GetUserByIdAndRoleIntegrationResponse> Handle(GetUserByIdAndRoleIntegrationQuery integrationQuery,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(integrationQuery.UserId, cancellationToken);
            if (user is null)
                throw new NotFoundException(nameof(User), integrationQuery.UserId);

            var account = await userRepository.GetAccountByUserId(integrationQuery.UserId);

            if (account is null)
                throw new NotFoundException(nameof(Account), integrationQuery.UserId);

            if (account.Role != integrationQuery.Role)
                throw new NotFoundException($"Role {integrationQuery.Role} not found for user",
                    integrationQuery.UserId);

            var response = new GetUserByIdAndRoleIntegrationResponse(
                user.Id,
                account.Id,
                account.Email,
                user.FullName,
                user.PhoneNumber,
                account.Role,
                account.Status.ToString(),
                account.CreatedAt
            );

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}