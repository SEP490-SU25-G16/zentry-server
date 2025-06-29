using Zentry.Modules.ConfigurationManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ConfigurationManagement.Application.Abstractions;

public interface IConfigurationRepository : IRepository<Configuration, Guid>
{
    Task<Configuration?> GetByKeyAsync(string key, CancellationToken cancellationToken);
}