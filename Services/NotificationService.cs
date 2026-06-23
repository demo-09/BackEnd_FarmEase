using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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
        // Prioritize .env variables, fallback to appsettings Smtp section
        var host =process.env.SMTP_HOST ?? _config["SMTP_HOST"] ?? _config["Smtp:Host"];
        var port =process.env.SMTP_PORT ?? _config["SMTP_PORT"] ?? _config["Smtp:Port"] ?? "587";
        var user =process.env.SMTP_USER ?? _config["SMTP_USER"] ?? _config["Smtp:Username"];
        var pass =process.env.SMTP_PASS ?? _config["SMTP_PASS"] ?? _config["Smtp:Password"];
        var from =process.env.SMTP_FROM ?? _config["SMTP_FROM"] ?? _config["Smtp:FromEmail"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            Console.WriteLine("[EMAIL ERROR]: SMTP configuration is incomplete. Please check your .env file.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("FarmEase", from));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            await client.ConnectAsync(host, int.Parse(port), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            watch.Stop();
            
            Console.WriteLine($"[EMAIL SENT to {toEmail}]: {subject} (Took {watch.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL FAILED to {toEmail}]");
            Console.WriteLine($"ERROR: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException.Message}");
            throw; 
        }
    }

    public async Task SendSmsAsync(string toNumber, string message)
    {
        // SMS implementation would go here if Twilio was configured
        Console.WriteLine($"[MOCK SMS to {toNumber}]: {message}");
        await Task.CompletedTask;
    }
}
