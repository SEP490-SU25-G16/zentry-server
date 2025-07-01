using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ConfigurationManagement.Abstractions;

public interface IConfigurationRepository : IRepository<Configuration, Guid>
{
    Task<Configuration?> GetByKeyAsync(string key, CancellationToken cancellationToken);
}