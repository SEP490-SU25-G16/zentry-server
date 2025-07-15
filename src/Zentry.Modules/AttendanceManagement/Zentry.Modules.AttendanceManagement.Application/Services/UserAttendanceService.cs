using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class UserAttendanceService(IMediator mediator) : IUserAttendanceService
{
    public async Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(string role, Guid userId,
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
