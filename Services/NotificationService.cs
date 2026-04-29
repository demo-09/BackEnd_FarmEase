using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace backEnd.Services;

public interface INotificationService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendSmsAsync(string toNumber, string message);
}

public class NotificationService : INotificationService
{
    private readonly IConfiguration _config;

    public NotificationService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpConfig = _config.GetSection("Smtp");
        var host = smtpConfig["Host"];
        var portStr = smtpConfig["Port"];
        var username = smtpConfig["Username"];
        var password = smtpConfig["Password"];
        var fromEmail = smtpConfig["FromEmail"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // Fallback for prototype if no SMTP configured
            Console.WriteLine($"[MOCK EMAIL to {toEmail}]: {subject} - {body}");
            return;
        }

        int port = int.TryParse(portStr, out int p) ? p : 587;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail ?? username),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        try 
        {
            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[EMAIL SUCCESS]: Sent to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR]: Failed to send to {toEmail}. Error: {ex.Message}");
            throw; // Re-throw to allow AuthService to catch it if needed
        }
    }

    public async Task SendSmsAsync(string toNumber, string message)
    {
        var twilioConfig = _config.GetSection("Twilio");
        var accountSid = twilioConfig["AccountSid"];
        var authToken = twilioConfig["AuthToken"];
        var fromNumber = twilioConfig["FromNumber"];

        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
        {
            // Fallback for prototype if no Twilio configured
            Console.WriteLine($"[MOCK SMS to {toNumber}]: {message}");
            return;
        }

        try 
        {
            TwilioClient.Init(accountSid, authToken);

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(toNumber)
            );
            Console.WriteLine($"[SMS SUCCESS]: SID {result.Sid} sent to {toNumber}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMS ERROR]: Failed to send to {toNumber}. Error: {ex.Message}");
            throw;
        }
    }
}
