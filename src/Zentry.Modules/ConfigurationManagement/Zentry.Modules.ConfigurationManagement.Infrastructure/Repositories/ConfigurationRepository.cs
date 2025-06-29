using Zentry.Modules.ConfigurationManagement.Application.Abstractions;
using Zentry.Modules.ConfigurationManagement.Domain.Entities;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    public Task<IEnumerable<Configuration>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Configuration?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Configuration entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Update(Configuration entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(Configuration entity)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Configuration?> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}