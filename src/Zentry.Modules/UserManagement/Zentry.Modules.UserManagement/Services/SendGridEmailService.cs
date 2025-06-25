// Zentry.Modules.UserManagement/Services/SendGridEmailService.cs
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace Zentry.Modules.UserManagement.Services;

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(IConfiguration configuration)
    {
        string sendGridApiKey = configuration["SendGrid:ApiKey"] ??
                                throw new ArgumentNullException("SendGrid API Key not configured.");
        _fromEmail = configuration["SendGrid:FromEmail"] ??
                     throw new ArgumentNullException("SendGrid FromEmail not configured.");
        _fromName = configuration["SendGrid:FromName"] ?? "Zentry Support"; // Default sender name

        _sendGridClient = new SendGridClient(sendGridApiKey);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var from = new EmailAddress(_fromEmail, _fromName);
        var toEmail = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, ""); // Plain text body, HTML body (empty for now)

        try
        {
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"SendGrid API Error: {response.StatusCode} - {response.Headers} - {responseBody}");
                throw new ApplicationException($"Failed to send email to {to}: {response.StatusCode}. Details: {responseBody}");
            }
            Console.WriteLine($"Email sent successfully to {to} via SendGrid.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception when sending email via SendGrid: {ex.Message}");
            throw; // Re-throw or handle as appropriate for your error logging
        }
    }
}
