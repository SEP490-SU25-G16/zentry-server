using Zentry.Modules.UserManagement.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.UserManagement.Interfaces;

public interface IUserRequestRepository : IRepository<UserRequest, Guid>
{
}