using MediatR;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Services;

public class UserScheduleService(IMediator mediator) : IUserScheduleService
{
    public async Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(Role role, Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdAndRoleIntegrationQuery(role, userId);
        var result = await mediator.Send(query, cancellationToken);
        return result;
    }
}
