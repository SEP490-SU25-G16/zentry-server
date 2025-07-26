using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class UserService(IMediator mediator) : IUserService
{
    public async Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(Role role, Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUserByIdAndRoleIntegrationQuery(role, userId);
            var result = await mediator.Send(query, cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}