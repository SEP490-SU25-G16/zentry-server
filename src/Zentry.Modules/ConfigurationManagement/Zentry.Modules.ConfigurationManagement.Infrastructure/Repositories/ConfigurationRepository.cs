using Zentry.Modules.ConfigurationManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    public Task<Domain.Entities.Configuration?> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Domain.Entities.Configuration> GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Domain.Entities.Configuration>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Domain.Entities.Configuration>> FindAsync(
        ISpecification<Domain.Entities.Configuration> specification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Domain.Entities.Configuration entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Domain.Entities.Configuration entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Domain.Entities.Configuration entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}