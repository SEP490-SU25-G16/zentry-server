namespace Zentry.Modules.UserManagement.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string role);
}
