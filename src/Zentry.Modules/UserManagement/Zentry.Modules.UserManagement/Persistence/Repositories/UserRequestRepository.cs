using Microsoft.EntityFrameworkCore;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.Modules.UserManagement.Interfaces;

namespace Zentry.Modules.UserManagement.Persistence.Repositories;

public class UserRequestRepository(UserDbContext dbContext) : IUserRequestRepository
{
    public async Task AddAsync(UserRequest userRequest, CancellationToken cancellationToken)
    {
        await dbContext.UserRequests.AddAsync(userRequest, cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<UserRequest> entities, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserRequest>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<UserRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.UserRequests.FirstOrDefaultAsync(ur => ur.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(UserRequest userRequest, CancellationToken cancellationToken)
    {
        dbContext.UserRequests.Update(userRequest);
    }

    public Task DeleteAsync(UserRequest entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

}
