using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ConfigurationManagement.Application.Abstractions;

public interface IConfigurationRepository : IRepository<Domain.Entities.Configuration>
{
    Task<Domain.Entities.Configuration?> GetByKeyAsync(string key, CancellationToken cancellationToken);
}