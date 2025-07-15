using MediatR;
using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Services;

public class UserDeviceService(IMediator mediator) : IUserDeviceService
{
    public async Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new CheckUserExistIntegrationQuery(userId);
            var result = await mediator.Send(query, cancellationToken);
            return result.IsExist;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
