namespace Zentry.Modules.UserManagement.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
