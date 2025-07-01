namespace Zentry.Modules.DeviceManagement.Application.Abstractions;

public interface IUserDeviceService
{
    Task<bool> CheckUserExistsAsync(Guid userId);
}
