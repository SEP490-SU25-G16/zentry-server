using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.UserManagement.Interfaces;

public interface IUserQueryService
{
    Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid userId, CancellationToken cancellationToken);
}
