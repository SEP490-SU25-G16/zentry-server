namespace Zentry.Modules.UserManagement.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
