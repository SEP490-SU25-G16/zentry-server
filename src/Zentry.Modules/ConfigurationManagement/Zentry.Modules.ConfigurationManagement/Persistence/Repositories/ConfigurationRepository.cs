using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;

namespace Zentry.Modules.ConfigurationManagement.Persistence.Repositories;

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

    public Task UpdateAsync(Configuration entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Configuration entity, CancellationToken cancellationToken)
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
