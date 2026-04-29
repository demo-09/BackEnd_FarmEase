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
        Console.WriteLine($"[MOCK EMAIL to {toEmail}]: {subject} - {body}");
        await Task.CompletedTask;
    }

    public async Task SendSmsAsync(string toNumber, string message)
    {
        Console.WriteLine($"[MOCK SMS to {toNumber}]: {message}");
        await Task.CompletedTask;
    }
}
